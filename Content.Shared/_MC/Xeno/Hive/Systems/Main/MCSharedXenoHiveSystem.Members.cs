using Content.Shared._MC.Xeno.Hive.Events;
using Content.Shared._RMC14.Sprite;
using Content.Shared._RMC14.Xenonids.Hive;

namespace Content.Shared._MC.Xeno.Hive.Systems.Main;

public abstract partial class MCSharedXenoHiveSystem
{
    [Dependency] private readonly SharedRMCSpriteSystem _rmcSprite = null!;

    public bool IsMember(Entity<HiveMemberComponent?> entity, EntityUid hiveUid)
    {
        if (GetHive(entity) is not { } memberHive)
            return false;

        return memberHive.Owner == hiveUid;
    }

    public void SetHive(Entity<HiveMemberComponent?> entity, EntityUid? hive)
    {
        var memberComponent = entity.Comp ?? EnsureComp<HiveMemberComponent>(entity);

        var oldHive = memberComponent.Hive;
        if (oldHive == hive)
            return;

        if (!_mcHiveQuery.TryComp(hive, out var hiveComp))
        {
            Log.Error($"Tried to set hive of {ToPrettyString(entity)} to bad hive entity {ToPrettyString(hive)}");
            return;
        }

        var hiveEntity = (hive.Value, hiveComp);

        memberComponent.Hive = hive;
        Dirty(entity);

        var color = hiveEntity.hiveComp.Color;
        if (color != Color.White)
            _rmcSprite.SetColor(entity.Owner, color);

        var ev = new MCXenoHiveChangedEvent(hiveEntity, oldHive);
        RaiseLocalEvent(entity, ref ev);
    }

    public bool HasHive(Entity<HiveMemberComponent?> member)
    {
        return GetHive(member) != null;
    }

    #region Same

    public void SetSameHive(Entity<HiveMemberComponent?> source, Entity<HiveMemberComponent?> destination)
    {
        if (GetHive(source) is { } hive)
            SetHive(destination, hive);
    }

    public bool FromSameHive(Entity<HiveMemberComponent?> source, Entity<HiveMemberComponent?> destination)
    {
        return GetHive(source) is { } aHive && IsMember(destination, aHive);
    }

    #endregion
}
