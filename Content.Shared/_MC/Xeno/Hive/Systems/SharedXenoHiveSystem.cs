using Content.Shared._MC.Xeno.Hive.Prototypes;
using Robust.Shared.Prototypes;
// ReSharper disable CheckNamespace

namespace Content.Shared._RMC14.Xenonids.Hive;

public partial class SharedXenoHiveSystem
{
    #region Psypoints

    public void AddPsypoints(Entity<HiveComponent> entity, ProtoId<MCXenoHivePsypointTypePrototype> id, int value)
    {
        SetPsypoints(entity, id, GetPsypoints(entity, id) + value);
    }

    public void AddPsypointsFromOwner(EntityUid uid, ProtoId<MCXenoHivePsypointTypePrototype> id, int value)
    {
        SetPsypointsFromOwner(uid, id, GetPsypointsFromOwner(uid, id) + value);
    }

    public void SetPsypoints(Entity<HiveComponent> entity, ProtoId<MCXenoHivePsypointTypePrototype> id, int value)
    {
        if (entity.Comp.Psypoints.TryAdd(id, value))
            return;

        entity.Comp.Psypoints[id] = value;
        Dirty(entity);
    }

    public void SetPsypointsFromOwner(EntityUid uid, ProtoId<MCXenoHivePsypointTypePrototype> id, int value)
    {
        if (!TryComp<HiveMemberComponent>(uid, out var hiveMemberComponent))
            return;

        if (!TryComp<HiveComponent>(hiveMemberComponent.Hive, out var hiveComponent))
            return;

        SetPsypoints((hiveMemberComponent.Hive.Value, hiveComponent), id, value);
    }

    public int GetPsypoints(Entity<HiveComponent> entity, ProtoId<MCXenoHivePsypointTypePrototype> id)
    {
        return entity.Comp.Psypoints.GetValueOrDefault(id, 0);
    }

    public int GetPsypointsFromOwner(EntityUid uid, ProtoId<MCXenoHivePsypointTypePrototype> id)
    {
        if (!TryComp<HiveMemberComponent>(uid, out var hiveMemberComponent))
            return 0;

        return !TryComp<HiveComponent>(hiveMemberComponent.Hive, out var hiveComponent)
            ? 0
            : GetPsypoints((hiveMemberComponent.Hive.Value, hiveComponent), id);
    }

    public bool HasPsypoints(Entity<HiveComponent> entity, ProtoId<MCXenoHivePsypointTypePrototype> id, int value)
    {
        if (!entity.Comp.Psypoints.TryGetValue(id, out var count))
            return false;

        return value < count;
    }

    public bool HasPsypointsFromOwner(EntityUid uid, ProtoId<MCXenoHivePsypointTypePrototype> id, int value)
    {
        return value < GetPsypointsFromOwner(uid, id);
    }

    #endregion

    #region LarvaPoints

    public void AddLarvaPointsOwner(EntityUid uid, int value)
    {
        SetLarvaPointsFromOwner(uid, GetLarvaPointsFromOwner(uid) + value);
    }

    public void AddLarvaPoints(Entity<HiveComponent> entity, int value)
    {
        SetLarvaPoints(entity, GetLarvaPoints(entity) + value);
    }

    public void SetLarvaPointsFromOwner(EntityUid uid, int value)
    {
        if (!TryComp<HiveMemberComponent>(uid, out var hiveMemberComponent))
            return;

        if (!TryComp<HiveComponent>(hiveMemberComponent.Hive, out var hiveComponent))
            return;

        SetLarvaPoints((hiveMemberComponent.Hive.Value, hiveComponent), value);
    }

    public void SetLarvaPoints(Entity<HiveComponent> entity, int value)
    {
        if (!entity.Comp.CanLarvaPoints)
            return;

        if (value >= entity.Comp.LarvaPointsPerBurrowedLarva)
        {
            IncreaseBurrowedLarva(entity, value / entity.Comp.LarvaPointsPerBurrowedLarva);
            value %= entity.Comp.LarvaPointsPerBurrowedLarva;
        }

        entity.Comp.LarvaPoints = value;
        Dirty(entity);
    }

    public int GetLarvaPoints(Entity<HiveComponent> entity)
    {
        return entity.Comp.LarvaPoints;
    }

    public void AddBurrowedLarvaCount(Entity<HiveComponent> entity, int value)
    {
        SetBurrowedLarvaCount(entity, GetBurrowedLarvaCount(entity) + value);
    }

    public void SetBurrowedLarvaCount(Entity<HiveComponent> entity, int value)
    {
        entity.Comp.BurrowedLarva = value;
        Dirty(entity);
    }

    public int GetBurrowedLarvaCount(Entity<HiveComponent> entity)
    {
        return entity.Comp.BurrowedLarva;
    }

    public int GetLarvaPointsFromOwner(EntityUid uid)
    {
        if (!TryComp<HiveMemberComponent>(uid, out var hiveMemberComponent))
            return 0;

        return !TryComp<HiveComponent>(hiveMemberComponent.Hive, out var hiveComponent)
            ? 0
            : GetLarvaPoints((hiveMemberComponent.Hive.Value, hiveComponent));
    }

    #endregion
}
