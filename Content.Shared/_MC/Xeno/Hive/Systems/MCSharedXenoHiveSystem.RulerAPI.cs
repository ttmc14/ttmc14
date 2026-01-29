using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Events;
using Content.Shared._RMC14.Xenonids.Announce;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Mobs;

namespace Content.Shared._MC.Xeno.Hive.Systems;

public abstract partial class MCSharedXenoHiveSystem
{
    public bool AddRuler(Entity<MCXenoHiveComponent?> entity, EntityUid rulerUid)
    {
        if (!_hiveQuery.Resolve(entity, ref entity.Comp))
            return false;

        if (entity.Comp.Rulers.Contains(rulerUid))
            return false;

        entity.Comp.Rulers.Add(rulerUid);
        Dirty(entity);

        var ev = new MCXenoHiveRulerAdded(rulerUid, entity.Owner);
        RaiseLocalEvent(entity, ref ev);

        _rmcXenoAnnounce.AnnounceSameHive(rulerUid, Loc.GetString("mc-xeno-hive-ruler-new"));
        return true;
    }

    public bool RemoveRuler(Entity<MCXenoHiveComponent?> entity, EntityUid rulerUid)
    {
        if (!_hiveQuery.Resolve(entity, ref entity.Comp))
            return false;

        var result = entity.Comp.Rulers.Remove(rulerUid);
        Dirty(entity);

        // _rmcXenoAnnounce.AnnounceSameHive(entity.Owner, Loc.GetString("mc-xeno-hive-ruler-dead"));

        return result;
    }

    public bool HasRuler(Entity<MCXenoHiveComponent?> entity)
    {
        if (!_hiveQuery.Resolve(entity, ref entity.Comp))
            return false;

        return entity.Comp.Rulers.Count > 0;
    }

    #region Member utilities

    public bool MemberAddRuler(EntityUid uid)
    {
        if (!_rmcHiveMemberQuery.TryComp(uid, out var hiveMemberComponent) || !hiveMemberComponent.Hive.HasValue)
            return false;

        return AddRuler(hiveMemberComponent.Hive.Value, uid);
    }

    public bool MemberRemoveRuler(EntityUid uid)
    {
        if (!_rmcHiveMemberQuery.TryComp(uid, out var hiveMemberComponent) || !hiveMemberComponent.Hive.HasValue)
            return false;

        return RemoveRuler(hiveMemberComponent.Hive.Value, uid);
    }

    public bool MemberHasRuler(EntityUid uid)
    {
        if (!_rmcHiveMemberQuery.TryComp(uid, out var hiveMemberComponent))
            return false;

        return hiveMemberComponent.Hive is not null && HasRuler(hiveMemberComponent.Hive.Value);
    }

    #endregion
}
