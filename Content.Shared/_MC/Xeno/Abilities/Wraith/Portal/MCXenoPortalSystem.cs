using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Item;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Teleportation.Components;
using Robust.Shared.Physics.Events;

namespace Content.Shared._MC.Xeno.Abilities.Wraith.Portal;

public sealed class MCXenoPortalSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly PullingSystem _pulling = null!;

    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;

    private const string PortalFixture = "portalFixture";

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoPortalComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<MCXenoPortalComponent, EndCollideEvent>(OnEndCollide);
    }

    private void OnCollide(Entity<MCXenoPortalComponent> entity, ref StartCollideEvent args)
    {
        if (args.OurFixtureId != PortalFixture)
            return;

        var target = args.OtherEntity;

        if (!HasComp<XenoComponent>(target) && !HasComp<ItemComponent>(target) && !HasComp<ProjectileComponent>(target))
            return;

        if (HasComp<XenoComponent>(target) && !_mcXenoHive.FromSameHive(entity.Owner, target))
            return;

        if (TryComp<PullableComponent>(target, out var pullable) && pullable.BeingPulled)
            _pulling.TryStopPull(target, pullable);

        if (TryComp<PullerComponent>(target, out var pullerComp) && TryComp<PullableComponent>(pullerComp.Pulling, out var subjectPulling))
            _pulling.TryStopPull(pullerComp.Pulling.Value, subjectPulling);

        if (HasComp<PortalTimeoutComponent>(target))
            return;

        if (entity.Comp.LinkedEntity is not { } linkedEntity)
            return;

        var timeout = EnsureComp<PortalTimeoutComponent>(target);
        timeout.EnteredPortal = entity;
        Dirty(target, timeout);

        _transform.SetCoordinates(target, Transform(linkedEntity).Coordinates);
    }

    private void OnEndCollide(Entity<MCXenoPortalComponent> entity, ref EndCollideEvent args)
    {
        if (args.OurFixtureId != PortalFixture)
            return;

        var target = args.OtherEntity;
        if (!HasComp<XenoComponent>(target))
            return;

        if (!_mcXenoHive.FromSameHive(entity.Owner, target))
            return;

        if (TryComp<PortalTimeoutComponent>(target, out var timeout) && timeout.EnteredPortal != entity)
            RemCompDeferred<PortalTimeoutComponent>(target);
    }
}
