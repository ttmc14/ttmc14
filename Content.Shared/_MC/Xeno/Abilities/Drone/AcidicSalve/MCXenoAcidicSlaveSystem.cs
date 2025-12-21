using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Sunder;
using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Drone.AcidicSalve;

public sealed class MCXenoAcidicSlaveSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = null!;

    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedInteractionSystem _interaction = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = null!;
    [Dependency] private readonly SharedRMCFlammableSystem _flammable = null!;

    [Dependency] private readonly MCXenoHealSystem _mcXenoHeal = null!;
    [Dependency] private readonly MCXenoSunderSystem _mcXenoSunder = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoAcidicSalveComponent, MCXenoAcidicSlaveActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoAcidicSalveComponent, MCXenoAcidicSlaveDoAfterEvent>(OnDoAfter);
    }

    private void OnAction(Entity<MCXenoAcidicSalveComponent> entity, ref MCXenoAcidicSlaveActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_interaction.InRangeUnobstructed(entity.Owner, args.Target, entity.Comp.Range))
            return;

        if (_mobState.IsDead(args.Target))
            return;

        if (_flammable.IsOnFire(args.Target))
            return;

        if (!_xenoHive.FromSameHive(entity.Owner, args.Target))
            return;

        if (!RMCActions.CanUseActionPopup(entity, args.Action, entity))
            return;

        var ev = new MCXenoAcidicSlaveDoAfterEvent(args.Action, EntityManager);
        var doAfter = new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, ev, entity, args.Target)
        {
            RequireCanInteract = false,
            DistanceThreshold = entity.Comp.Range,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfter(Entity<MCXenoAcidicSalveComponent> entity, ref MCXenoAcidicSlaveDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target is not {} target)
            return;

        var action = args.GetAction(EntityManager);
        if (!RMCActions.TryUseAction(entity, action, entity))
            return;

        args.Handled = true;

        var value = 50 + _mcXenoHeal.GetRecoveryAura(target) * _mcXenoHeal.GetMaxHealth(target) * 0.01f;

        _mcXenoHeal.Heal(target, value);
        _mcXenoSunder.AddSunder(target, value * 0.1f);
        _audio.PlayPredicted(entity.Comp.Sound, entity, entity);

        ActionStartUseDelay<MCXenoAcidicSlaveActionEvent>(entity);
        ServerSpawnAttachedTo(entity.Comp.EffectProtoId, args.Target.Value.ToCoordinates());
    }
}
