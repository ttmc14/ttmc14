using Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Components.Activation;
using Content.Shared._MC.Xeno.Abilities.Ravager.Ravage;
using Content.Shared._MC.Xeno.Abilities.Runner.Pounce;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Systems.Activation;

public sealed class MCXenoRageActivationHealthSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _plasma = null!;
    [Dependency] private readonly MCXenoRageSystem _rage = null!;
    [Dependency] private readonly MCXenoHealSystem _heal = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoRageActivationHealthComponent, DamageChangedEvent>(OnHealthDamageChanged);
    }

    private void OnHealthDamageChanged(Entity<MCXenoRageActivationHealthComponent> entity, ref DamageChangedEvent args)
    {
        if (!_heal.CheckHealthThreshold(entity, entity.Comp.MinHealthThreshold))
        {
            _rage.Deactivate(entity);
            return;
        }

        _rage.Activate(entity);
        _rage.SetPower(entity, CalculatePowerAction(entity));

        var health = _heal.GetHealth(entity);
        if (health >= 0 || entity.Comp.SpecialUsed)
            return;

        _popup.PopupClient(Loc.GetString("mc-xeno-ability-rage-rip-and-tear"), entity, entity, PopupType.LargeCaution);
        _plasma.RegenPlasmaMax(entity.Owner);

        ActionClearUseDelay<MCXenoRavageActionEvent>(entity);
        ActionClearUseDelay<MCXenoPounceActionEvent>(entity);

        entity.Comp.SpecialUsed = true;
    }

    private float CalculatePowerAction(Entity<MCXenoRageActivationHealthComponent> entity)
    {
        var health = _heal.GetHealth(entity);
        var max = _heal.GetMaxHealth(entity);
        var alive = _heal.GetHealthAlive(entity);

        var endureHealthLimit = alive - max;
        var rageThreshold = max * (1 - entity.Comp.MinHealthThreshold);

        return float.Max(0, 1 - (health - endureHealthLimit) / (max - endureHealthLimit - rageThreshold));
    }
}
