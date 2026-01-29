using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Prototypes;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Hive.Systems;

public abstract partial class MCSharedXenoHiveSystem
{
    [PublicAPI]
    public void AddPsypoints(Entity<MCXenoHiveComponent?> entity, ProtoId<MCXenoHivePsypointTypePrototype> id, int value)
    {
        if (!_hiveQuery.Resolve(entity, ref entity.Comp))
            return;

        SetPsypoints(entity, id, GetPsypoints(entity, id) + value);
    }

    [PublicAPI]
    public void SetPsypoints(Entity<MCXenoHiveComponent?> entity, ProtoId<MCXenoHivePsypointTypePrototype> id, int value)
    {
        if (!_hiveQuery.Resolve(entity, ref entity.Comp))
            return;

        if (entity.Comp.Psypoints.TryAdd(id, value))
            return;

        entity.Comp.Psypoints[id] = value;
        Dirty(entity);
    }

    [PublicAPI]
    public int GetPsypoints(Entity<MCXenoHiveComponent?> entity, ProtoId<MCXenoHivePsypointTypePrototype> id)
    {
        if (!_hiveQuery.Resolve(entity, ref entity.Comp))
            return 0;

        return entity.Comp.Psypoints.GetValueOrDefault(id, 0);
    }

    [PublicAPI]
    public bool HasPsypoints(Entity<MCXenoHiveComponent?> entity, ProtoId<MCXenoHivePsypointTypePrototype> id, int value)
    {
        return value < GetPsypoints(entity, id);
    }

    #region Member utilities

    [PublicAPI]
    public void MemberAddPsypoints(EntityUid uid, ProtoId<MCXenoHivePsypointTypePrototype> id, int value)
    {
        MemberSetPsypoints(uid, id, MemberGetPsypoints(uid, id) + value);
    }

    [PublicAPI]
    public void MemberSetPsypoints(EntityUid uid, ProtoId<MCXenoHivePsypointTypePrototype> id, int value)
    {
        if (!_rmcHiveMemberQuery.TryComp(uid, out var component) || component.Hive is not {} hiveUid)
            return;

        SetPsypoints(hiveUid, id, value);
    }

    [PublicAPI]
    public int MemberGetPsypoints(EntityUid uid, ProtoId<MCXenoHivePsypointTypePrototype> id)
    {
        if (!_rmcHiveMemberQuery.TryComp(uid, out var component) || component.Hive is not {} hiveUid)
            return 0;

        return GetPsypoints(hiveUid, id);
    }

    [PublicAPI]
    public bool MemberHasPsypoints(EntityUid uid, ProtoId<MCXenoHivePsypointTypePrototype> id, int value)
    {
        if (!_rmcHiveMemberQuery.TryComp(uid, out var component) || component.Hive is not {} hiveUid)
            return false;

        return HasPsypoints(hiveUid, id, value);
    }

    #endregion
}
