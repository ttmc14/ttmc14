using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Sunder;
using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared._MC.Xeno.Abilities.Shrike.PsychicCure;

public sealed class MCXenoPsychicCureSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly MobStateSystem _mobState = null!;

    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedInteractionSystem _interaction = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedRMCFlammableSystem _flammable = null!;

    [Dependency] private readonly MCXenoHealSystem _mcXenoHeal = null!;
    [Dependency] private readonly MCXenoSunderSystem _mcXenoSunder = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPsychicCureComponent, MCXenoPsychicCureActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoPsychicCureComponent, MCXenoPsychicCureDoAfterEvent>(OnDoAfter);
    }

    private void OnAction(Entity<MCXenoPsychicCureComponent> entity, ref MCXenoPsychicCureActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_interaction.InRangeUnobstructed(entity.Owner, args.Target, entity.Comp.Range))
            return;

        if (_mobState.IsDead(args.Target))
            return;

        if (_flammable.IsOnFire(args.Target))
            return;

        if (!MCXenoHive.FromSameHive(entity.Owner, args.Target))
            return;

        if (!RMCActions.CanUseActionPopup(entity, args.Action, entity))
            return;

        var ev = new MCXenoPsychicCureDoAfterEvent(GetNetEntity(args.Action));
        var doAfter = new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, ev, entity, args.Target)
        {
            RequireCanInteract = false,
            DistanceThreshold = entity.Comp.Range,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfter(Entity<MCXenoPsychicCureComponent> entity, ref MCXenoPsychicCureDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (args.Target is not {} target)
            return;

        var action = GetEntity(args.Action);
        if (!RMCActions.TryUseAction(entity, action, entity))
            return;

        args.Handled = true;

        _mcXenoHeal.HealWounds(target, 10, powerScaling: false);
        _mcXenoSunder.AddSunder(target, 10);
        _audio.PlayPredicted(entity.Comp.Sound, entity, entity);

        ActionStartUseDelay<MCXenoPsychicCureActionEvent>(entity);

        if (_net.IsClient)
            return;

        SpawnAttachedTo(entity.Comp.EffectProtoId, args.Target.Value.ToCoordinates());
    }
}
