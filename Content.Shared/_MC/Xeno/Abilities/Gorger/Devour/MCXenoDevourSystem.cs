using Content.Shared._MC.Xeno.Abilities.Gorger.Devour.Components;
using Content.Shared._MC.Xeno.Abilities.Gorger.Devour.Events;
using Content.Shared._RMC14.Medical.Unrevivable;
using Content.Shared.Buckle.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Devour;

public sealed class MCXenoDevourSystem : MCXenoAbilitySystem
{
    /// <summary>
    /// Container slot that holds the swallowed corpse until it is released.
    /// </summary>
    private const string Stomach = "mc_gorger_stomach";

    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly RMCUnrevivableSystem _unrevivable = null!;

    [Dependency] private readonly MobStateSystem _mobState = null!;

    [Dependency] private readonly SharedContainerSystem _containers = null!;

    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoDevourComponent, MCXenoDevourActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoDevourComponent, MCXenoDevourDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<MCXenoDevourComponent, MobStateChangedEvent>(OnState);
        SubscribeLocalEvent<MCXenoDevourComponent, EntityTerminatingEvent>(OnTerminating);
    }

    private bool CanSwallow(EntityUid owner, EntityUid target) =>
        !_containers.IsEntityInContainer(target) && IsOrganicHuman(target) && IsDead(target) && _unrevivable.IsUnrevivable(target)
        && !MCFlammable.OnFire(owner) && CompOrNull<BuckleComponent>(target)?.BuckledTo is null
        && (!_containers.TryGetContainer(owner, Stomach, out var stomach) || stomach.Count == 0);

    private void OnAction(Entity<MCXenoDevourComponent> ent, ref MCXenoDevourActionEvent args)
    {
        if (args.Handled || !CanUseAction(ent, args.Action))
            return;

        var eject = args.Target == ent.Owner;
        if (eject && (!_containers.TryGetContainer(ent, Stomach, out var stomach) || stomach.Count == 0))
            return;

        if (!eject && !CanSwallow(ent, args.Target))
            return;

        var ev = new MCXenoDevourDoAfterEvent(args.Action, EntityManager);
        var doAfter = new DoAfterArgs(EntityManager, ent, eject ? ent.Comp.RegurgitateDelay : ent.Comp.DevourDelay, ev, ent, args.Target)
        {
            DistanceThreshold = ent.Comp.Range,
            AttemptFrequency = eject ? AttemptFrequency.Never : AttemptFrequency.StartAndEnd,
            BreakOnMove = true,
            DuplicateCondition = DuplicateConditions.None,
            RequireCanInteract = false,
        };

        args.Handled = _doAfter.TryStartDoAfter(doAfter);

        if (args.Handled && Net.IsServer)
            ent.Comp.ChannelAudio = _audio.PlayPvs(eject ? ent.Comp.ReleaseSound : ent.Comp.SwallowSound, ent)?.Entity;
    }

    private void OnDoAfter(Entity<MCXenoDevourComponent> ent, ref MCXenoDevourDoAfterEvent args)
    {
        ent.Comp.ChannelAudio = _audio.Stop(ent.Comp.ChannelAudio);
        if (args.Cancelled || !_mobState.IsAlive(ent) || args.Target is not { } target)
            return;

        if (target == ent.Owner)
        {
            Regurgitate(ent);
            return;
        }

        if (Net.IsClient || !CanSwallow(ent, target))
            return;

        _containers.Insert(target, _containers.EnsureContainer<ContainerSlot>(ent, Stomach));
    }

    private void OnState(Entity<MCXenoDevourComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            Regurgitate(ent);
    }

    private void OnTerminating(Entity<MCXenoDevourComponent> ent, ref EntityTerminatingEvent args)
    {
        ent.Comp.ChannelAudio = _audio.Stop(ent.Comp.ChannelAudio);
        Regurgitate(ent);
    }

    private void Regurgitate(EntityUid owner)
    {
        if (Net.IsServer && _containers.TryGetContainer(owner, Stomach, out var stomach))
            _containers.EmptyContainer(stomach, force: true, destination: Transform(owner).Coordinates);
    }
}
