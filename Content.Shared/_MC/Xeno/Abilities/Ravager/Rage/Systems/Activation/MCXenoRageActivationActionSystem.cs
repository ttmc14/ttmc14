using System.Linq;
using Content.Shared._MC.Xeno.Abilities.Ravager.Endure;
using Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Components;
using Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Components.Activation;
using Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Events;
using Content.Shared._MC.Xeno.Abilities.Ravager.Ravage;
using Content.Shared._MC.Xeno.Abilities.Runner.Pounce;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared._MC.Xeno.Sunder;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Systems.Activation;

public sealed class MCXenoRageActivationSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = null!;
    [Dependency] private readonly MCXenoEndureSystem _endure = null!;
    [Dependency] private readonly MCXenoSunderSystem _sunder = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _plasma = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly MCXenoRageSystem _rage = null!;
    [Dependency] private readonly MCXenoHealSystem _heal = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoRageActivationActionComponent, MCXenoRageActiveActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoRageActivationActionComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
        SubscribeLocalEvent<MCXenoRageActivationActionComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
        SubscribeLocalEvent<MCXenoRageActivationActionComponent, MeleeHitEvent>(OnMeleeHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoRageActiveComponent, MCXenoRageActivationActionComponent>();
        while (query.MoveNext(out var uid, out _, out var actionComponent))
        {
            if (actionComponent.RageTimeEnd > _timing.CurTime)
                continue;

            _rage.DeactivateDeferred(uid);
        }
    }

    private void OnAction(Entity<MCXenoRageActivationActionComponent> entity, ref MCXenoRageActiveActionEvent args)
    {
        if (!_heal.CheckHealthThreshold(entity, entity.Comp.MinHealthThreshold))
            return;

        var power = CalculatePowerAction(entity);

        _rage.Activate(entity);
        _rage.SetPower(entity, power);

        entity.Comp.RageTimeEnd = _timing.CurTime + entity.Comp.RageDuration;

        if (power >= entity.Comp.RageSuperRageThreshold)
        {
            _popup.PopupClient(Loc.GetString("mc-xeno-ability-rage-rip-and-tear"), entity, entity, PopupType.LargeCaution);
            _plasma.RegenPlasmaMax(entity.Owner);

            ActionClearUseDelay<MCXenoRavageActionEvent>(entity);
            ActionClearUseDelay<MCXenoPounceActionEvent>(entity);
        }

        var plasma = _plasma.GetPlasma(entity);
        var plasmaMax = _plasma.GetMaxPlasma(entity);
        var plasmaRage = float.Max(plasmaMax - plasma, plasmaMax * power);

        _plasma.RegenPlasma(entity, plasmaRage);

        var sunder = _sunder.GetSunder(entity.Owner);
        var sunderRage = float.Max(sunder, 100 * power);

        _sunder.AddSunder(entity.Owner, sunderRage);

        _movementSpeedModifier.RefreshMovementSpeedModifiers(entity);

        args.Handled = true;
    }

    private void OnGetMeleeDamage(Entity<MCXenoRageActivationActionComponent> entity, ref GetMeleeDamageEvent args)
    {
        if (!_rage.IsActive(entity))
            return;

        args.Damage *= 1 + _rage.GetPower(entity);
    }

    private void OnRefreshMovementSpeed(Entity<MCXenoRageActivationActionComponent> entity, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!_rage.IsActive(entity))
            return;

        args.ModifySpeed(1f + 0.5f * _rage.GetPower(entity));
    }

    private void OnMeleeHit(Entity<MCXenoRageActivationActionComponent> entity, ref MeleeHitEvent args)
    {
        if (!_rage.IsActive(entity))
            return;

        var damage = (args.BaseDamage + args.BonusDamage)
            .GetTotal()
            .Float();

        var power = _rage.GetPower(entity);
        var count = args.HitEntities.Count(HasComp<MobStateComponent>);

        _heal.Heal(entity, damage * power * count);
        _endure.ExtendDuration(entity, TimeSpan.FromSeconds(count * 2f));
    }

    private float CalculatePowerAction(Entity<MCXenoRageActivationActionComponent> entity)
    {
        var health = _heal.GetHealth(entity);
        var alive = _heal.GetHealthAlive(entity);

        if (health <= 0)
            return entity.Comp.RagePowerMultiplier;

        return (1 - health / alive) * entity.Comp.RagePowerMultiplier;
    }
}
