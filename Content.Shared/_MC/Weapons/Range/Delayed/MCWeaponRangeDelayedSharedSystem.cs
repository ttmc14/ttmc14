using System.Diagnostics.CodeAnalysis;
using Content.Shared._MC.Weapons.Range.Delayed.Components;
using Content.Shared._MC.Weapons.Range.Delayed.Events;
using Content.Shared._RMC14.Movement;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Weapons.Range.Delayed;

public abstract class MCWeaponRangeDelayedSharedSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] protected readonly SharedGunSystem Gun = null!;
    [Dependency] protected readonly SharedRMCLagCompensationSystem RMCLagCompensation = null!;

    protected Dictionary<EntityUid, Entry> Entries = new();
    protected List<EntityUid> Remove = new();

    public override void Initialize()
    {
        SubscribeNetworkEvent<MCWeaponRangeDelayedRequestStartEvent>(OnShootStart);
        SubscribeNetworkEvent<MCWeaponRangeDelayedRequestStopEvent>(OnShootStop);
    }

    public override void Update(float frameTime)
    {
        Remove.Clear();

        foreach (var (uid, entry) in Entries)
        {
            if (_timing.CurTime < entry.TimeNext)
                continue;

            Remove.Add(uid);

            if (!TryComp<GunComponent>(uid, out var gunComponent))
                continue;

            RemComp<MCWeaponRangeDelayedAlertComponent>(entry.User);
            Gun.AttemptShoot((uid, gunComponent), entry.User, entry.Coordinates);
        }

        foreach (var uid in Remove)
        {
            Entries.Remove(uid);
        }
    }

    public void OnShootStart(MCWeaponRangeDelayedRequestStartEvent ev, EntitySessionEventArgs args)
    {
        var gunUid = GetEntity(ev.Gun);
        if (!TryComp<MCWeaponRangeDelayedComponent>(gunUid,  out var delayedComponent))
            return;

        if (args.SenderSession.AttachedEntity is not { } user)
            return;

        var coordinates = GetCoordinates(ev.Coordinates);
        var target = GetEntity(ev.Target);

        if (!Entries.TryGetValue(gunUid, out var entry))
        {
            var alert = EnsureComp<MCWeaponRangeDelayedAlertComponent>(user);
            alert.TimeStart = _timing.CurTime;
            alert.TimeEnd = _timing.CurTime + delayedComponent.Delay;
            Dirty(user, alert);

            Entries[gunUid] = new Entry(
                _timing.CurTime + delayedComponent.Delay,
                user,
                coordinates,
                target
            );
            return;
        }

        Entries[gunUid] = new Entry(
            entry.TimeNext,
            user,
            coordinates,
            target
        );
    }

    public void OnShootStop(MCWeaponRangeDelayedRequestStopEvent ev, EntitySessionEventArgs args)
    {
        var gunEntityUid = GetEntity(ev.Gun);
        Entries.Remove(gunEntityUid);

        if (args.SenderSession.AttachedEntity is not { } user)
            return;

        RemComp<MCWeaponRangeDelayedAlertComponent>(user);
    }

    [PublicAPI]
    public bool TryGetGun(
        EntityUid entity,
        out EntityUid gunEntity,
        [NotNullWhen(true)] out GunComponent? gunComponent,
        [NotNullWhen(true)] out MCWeaponRangeDelayedComponent? delayedComponent)
    {
        delayedComponent = null;
        return Gun.TryGetGun(entity, out gunEntity, out gunComponent) && TryComp(gunEntity, out delayedComponent);
    }

    public readonly record struct Entry(TimeSpan TimeNext, EntityUid User, EntityCoordinates Coordinates, EntityUid? Target);
}
