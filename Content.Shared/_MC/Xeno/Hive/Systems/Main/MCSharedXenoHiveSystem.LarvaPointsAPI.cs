using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Events;
using JetBrains.Annotations;

namespace Content.Shared._MC.Xeno.Hive.Systems.Main;

public abstract partial class MCSharedXenoHiveSystem
{
    #region Burrowed larvas

    [PublicAPI]
    public void AddBurrowedLarva(Entity<MCXenoHiveComponent?> entity, int value)
    {
        SetBurrowedLarva(entity, GetBurrowedLarva(entity) + value);
    }

    [PublicAPI]
    public void SetBurrowedLarva(Entity<MCXenoHiveComponent?> entity, int value)
    {
        if (!_mcHiveQuery.Resolve(entity, ref entity.Comp))
            return;

        entity.Comp.BurrowedLarva = value;
        Dirty(entity);
    }

    [PublicAPI]
    public int GetBurrowedLarva(Entity<MCXenoHiveComponent?> entity)
    {
        return !_mcHiveQuery.Resolve(entity, ref entity.Comp) ? 0 : entity.Comp.BurrowedLarva;
    }

    #endregion

    #region Larva points

    [PublicAPI]
    public void AddLarva(Entity<MCXenoHiveComponent?> entity, int value, bool bypassConfiguration = false)
    {
        if (!_mcHiveQuery.Resolve(entity, ref entity.Comp))
            return;

        SetLarva(entity, GetLarva(entity) + value, bypassConfiguration);
    }

    [PublicAPI]
    public void SetLarva(Entity<MCXenoHiveComponent?> entity, int value, bool bypassConfiguration = false)
    {
        if (!_mcHiveQuery.Resolve(entity, ref entity.Comp))
            return;

        if (!entity.Comp.Configuration.General.AllowHarvestLarvaPoints && !bypassConfiguration)
            return;

        var previous = entity.Comp.LarvaPoints;

        entity.Comp.LarvaPoints = value;
        Dirty(entity);

        var ev = new MCXenoHiveLarvaPointsChanged(previous, value);
        RaiseLocalEvent(entity, ref ev);
    }

    [PublicAPI]
    public int GetLarva(Entity<MCXenoHiveComponent?> entity)
    {
        return !_mcHiveQuery.Resolve(entity, ref entity.Comp) ? 0 : entity.Comp.LarvaPoints;
    }

    #endregion

    #region Member utilities

    [PublicAPI]
    public void MemberAddLarva(EntityUid uid, int value, bool bypassConfiguration = false)
    {
        MemberSetLarva(uid, MemberGetLarva(uid) + value, bypassConfiguration);
    }

    [PublicAPI]
    public void MemberSetLarva(EntityUid uid, int value, bool bypassConfiguration = false)
    {
        if (!_rmcHiveMemberQuery.TryComp(uid, out var component) || component.Hive is not {} hiveUid)
            return;

        SetLarva(hiveUid, value, bypassConfiguration);
    }

    [PublicAPI]
    public int MemberGetLarva(EntityUid uid)
    {
        if (!_rmcHiveMemberQuery.TryComp(uid, out var component) || component.Hive is not {} hiveUid)
            return 0;

        return GetLarva(hiveUid);
    }

    #endregion
}
