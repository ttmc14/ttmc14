using Content.Shared._MC.Xeno.Constructions.Silo;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Events;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using JetBrains.Annotations;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Hive.Systems.Main;

public abstract partial class MCSharedXenoHiveSystem : MCEntitySystemSingleton<MCXenoHiveSingletonComponent>
{
    [Dependency] private readonly MobStateSystem _mobState = null!;

    private EntityQuery<MCXenoHiveComponent> _mcHiveQuery;

    private EntityQuery<MobStateComponent> _mobStateQuery;
    private EntityQuery<HiveMemberComponent> _rmcHiveMemberQuery;

    [ViewVariables]
    public EntityUid? DefaultHive => Inst.Comp.DefaultHive;

    public override void Initialize()
    {
        base.Initialize();

        _mcHiveQuery = GetEntityQuery<MCXenoHiveComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();
        _rmcHiveMemberQuery = GetEntityQuery<HiveMemberComponent>();

        InitializeRuler();
    }

    public bool TryGetHive(Entity<HiveMemberComponent?> member, out Entity<MCXenoHiveComponent> hive)
    {
        hive = default;

        if (GetHive(member) is not { } uid)
            return false;

        hive = uid;
        return true;
    }

    public Entity<MCXenoHiveComponent>? GetHive(Entity<HiveMemberComponent?> member)
    {
        if (!_rmcHiveMemberQuery.Resolve(member, ref member.Comp, false))
            return null;

        if (member.Comp.Hive is not { } uid || TerminatingOrDeleted(uid))
            return null;

        if (!_mcHiveQuery.TryComp(uid, out var comp))
            return null;

        return (uid, comp);
    }

    [PublicAPI]
    public Dictionary<int, int> GetTiers(EntityUid hive)
    {
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

    public int GetLivingXenos(EntityUid hive, int minTier = 1)
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
        while (query.MoveNext(out var uid, out _))
        {
            if (!IsMember(uid, hive))
                continue;

            return true;
        }

        return false;
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
