using System.Linq;
using Content.Shared._MC.Engineering.Vending.Components;
using Content.Shared._MC.Engineering.Vending.Events;
using Content.Shared._RMC14.Vendors;
using Content.Shared.Mind;
using Content.Shared.Roles.Jobs;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Engineering.Vending;

public sealed class MCVendingSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedJobSystem _job = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCVendorItemVendedEvent>(OnItemVended);
    }

    private void OnItemVended(ref MCVendorItemVendedEvent ev)
    {
        var prototype = MetaData(ev.VendorUid).EntityPrototype?.ID;
        if (prototype is null)
            return;

        var component = EnsureComp<MCVendorItemComponent>(ev.ItemUid);

        component.VendorProtoId = prototype;
        DirtyField(ev.ItemUid, component, nameof(MCVendorItemComponent.VendorProtoId));
    }

    [PublicAPI]
    public EntityUid? Vend(EntityUid vendorUid, EntProtoId itemProtoId, EntityCoordinates itemCoordinates, EntityUid? actorUid = null)
    {
        if (actorUid is null)
            return null;

        var tuple = GetVendorItemInfo(vendorUid, itemProtoId);
        if (tuple is null)
            return null;

        var (entry, section) = tuple.Value;
        var user = EnsureComp<CMVendorUserComponent>(actorUid.Value);

        if (!CanVend(actorUid.Value, entry, section))
            return null;

        if (!HasValidJob(vendorUid, actorUid.Value))
            return null;

        // Points processing
        if (entry.Points is not null)
            user.Points -= entry.Points.Value;

        // Choice processing
        if (section.Choices is { } choices)
        {
            user.Choices.TryGetValue(choices.Id, out var used);
            user.Choices[choices.Id] = used + 1;
        }

        Dirty(actorUid.Value, user);

        var uid = Spawn(itemProtoId, itemCoordinates);
        if (entry.Amount is null)
            return uid;

        var ev = new MCVendorItemAmountEvent(
            vendorUid,
            actorUid.Value,
            entry.Id,
            1,
            isInfinite: false);

        RaiseLocalEvent(ref ev);

        return uid;
    }

    [PublicAPI]
    public bool CanVend(Entity<CMVendorUserComponent?> user, CMVendorEntry entry, CMVendorSection section)
    {
        if (!Resolve(user, ref user.Comp, false))
            return false;

        if (entry.Amount is not null && entry.Amount <= 0)
            return false;

        if (entry.Points is not null)
        {
            if (user.Comp is null || user.Comp.Points < entry.Points.Value)
                return false;
        }

        if (section.Choices is { } choices)
        {
            if (user.Comp is null)
                return false;

            if (user.Comp.Choices.TryGetValue(choices.Id, out var used) && used >= choices.Amount)
                return false;
        }

        return true;
    }

    [PublicAPI]
    public (CMVendorEntry, CMVendorSection)? GetVendorItemInfo(EntityUid vendorUid, EntProtoId itemProtoId)
    {
        if (!TryComp<CMAutomatedVendorComponent>(vendorUid, out var component))
            return null;

        foreach (var section in component.Sections)
        {
            foreach (var entry in section.Entries.Where(entry => entry.Id == itemProtoId))
            {
                return (entry, section);
            }
        }

        return null;
    }

    [PublicAPI]
    public EntityUid? GetVendorFirst(EntProtoId vendorProtoId, MapId mapId)
    {
        var query = EntityQueryEnumerator<CMAutomatedVendorComponent, MetaDataComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var metaDataComponent, out var transformComponent))
        {
            var prototype = metaDataComponent.EntityPrototype?.ID;
            if (prototype is null || prototype != vendorProtoId)
                continue;

            if (transformComponent.MapID != mapId)
                continue;

            return uid;
        }

        return null;
    }

    [PublicAPI]
    public bool HasValidJob(EntityUid vendorUid, Entity<CMVendorUserComponent?> user)
    {
        if (!Resolve(user, ref user.Comp, false))
            return false;

        if (!TryComp<CMAutomatedVendorComponent>(vendorUid, out var vendor))
            return false;

        if (vendor.Jobs.Count == 0)
            return true;

        _mind.TryGetMind(user.Owner, out var mindId, out _);
        foreach (var job in vendor.Jobs)
        {
            if (mindId.Valid && _job.MindHasJobWithId(mindId, job.Id))
                return true;

            if (user.Comp?.Id == job)
                return true;
        }

        return false;
    }
}
