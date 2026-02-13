using Content.Shared._MC.Xeno.Constructions.Silo;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Hive.Systems;

public abstract partial class MCSharedXenoHiveSystem : MCEntitySystemSingleton<MCXenoHiveSingletonComponent>
{
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly SharedXenoHiveSystem _rmcHive = null!;

    private EntityQuery<MCXenoHiveComponent> _hiveQuery;
    private EntityQuery<MobStateComponent> _mobStateQuery;
    private EntityQuery<HiveComponent> _rmcHiveQuery;
    private EntityQuery<HiveMemberComponent> _rmcHiveMemberQuery;

    [ViewVariables]
    public EntityUid? DefaultHive => Inst.Comp.DefaultHive;

    public override void Initialize()
    {
        base.Initialize();

        _hiveQuery = GetEntityQuery<MCXenoHiveComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();
        _rmcHiveQuery = GetEntityQuery<HiveComponent>();
        _rmcHiveMemberQuery = GetEntityQuery<HiveMemberComponent>();

        InitializeRuler();
    }

    public Entity<MCXenoHiveComponent>? GetHive(Entity<HiveMemberComponent?> member)
    {
        if (!_rmcHiveMemberQuery.Resolve(member, ref member.Comp, false))
            return null;

        if (member.Comp.Hive is not { } uid || TerminatingOrDeleted(uid))
            return null;

        if (!_hiveQuery.TryComp(uid, out var comp))
            return null;

        return (uid, comp);
    }

    public Dictionary<int, int> GetTiers(EntityUid hive)
    {
        if (!_rmcHiveQuery.TryComp(hive, out var component))
            return new Dictionary<int, int>();

        var result = new Dictionary<int, int>();
        var query = EntityQueryEnumerator<XenoComponent, HiveMemberComponent>();
        while (query.MoveNext(out var uid, out var xenoComponent, out _))
        {
            if (_mobState.IsDead(uid))
                continue;

            if (!result.TryAdd(xenoComponent.Tier, 0))
                result[xenoComponent.Tier]++;
        }

        return result;
    }

    public int GetLiving(EntityUid hive, int minTier = 1)
    {
        var total = 0;
        var query = EntityQueryEnumerator<XenoComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_mobState.IsDead(uid))
                continue;

            if (_rmcHiveMemberQuery.TryComp(uid, out var hiveMemberComponent) && hiveMemberComponent.Hive != hive)
                continue;

            if (comp.Tier < minTier)
                continue;

            total++;
        }

        return total;
    }

    public bool HasSilo(Entity<MCXenoHiveComponent> hive)
    {
        var query = EntityQueryEnumerator<MCXenoSiloComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!IsMember(uid, hive))
                continue;

            return true;
        }

        return false;
    }

    public bool IsMember(Entity<HiveMemberComponent?> entity, EntityUid hiveUid)
    {
        return _rmcHive.IsMember(entity, hiveUid);
    }

    public void SetHive(Entity<HiveMemberComponent?> entity, EntityUid? hive)
    {
        _rmcHive.SetHive(entity, hive);
    }

    public void SetSameHive(Entity<HiveMemberComponent?> src, Entity<HiveMemberComponent?> dest)
    {
        if (GetHive(src) is {} hive)
            SetHive(dest, hive);
    }

    public bool FromSameHive(Entity<HiveMemberComponent?> a, Entity<HiveMemberComponent?> b)
    {
        return GetHive(a) is {} aHive && IsMember(b, aHive);
    }
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoHiveSingletonComponent : Component
{
    #region Default hive

    [DataField, AutoNetworkedField]
    public string DefaultHiveName = "xeno hive";

    [DataField, AutoNetworkedField]
    public EntProtoId DefaultHiveId = "MCXenoHive";

    [DataField, AutoNetworkedField]
    public EntityUid? DefaultHive;

    #endregion
}
