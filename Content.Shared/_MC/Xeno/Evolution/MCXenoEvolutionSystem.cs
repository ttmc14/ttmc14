using Content.Shared._MC.Xeno.Evolution.Components;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Systems;
using Content.Shared._RMC14.Xenonids.Evolution;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Climbing.Systems;
using Content.Shared.Popups;
using Content.Shared.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Evolution;

public sealed partial class MCXenoEvolutionSystem : EntitySystem
{
    private static readonly TimeSpan EvolutionTickInterval = TimeSpan.FromSeconds(1);

    [Dependency] private readonly IComponentFactory _compFactory = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly IPrototypeManager _prototype = null!;
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly ClimbSystem _climb = null!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;

    [Dependency] private readonly XenoEvolutionSystem _rmcEvolution = null!;

    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;

    private readonly HashSet<EntityUid> _climbableTemp = new();
    private readonly HashSet<EntityUid> _intersectingTemp = new();

    private EntityQuery<MCXenoEvolutionAffectGainComponent> _evolutionAffectGainQuery;
    private EntityQuery<HiveMemberComponent> _hiveMemberQuery;

    public override void Initialize()
    {
        base.Initialize();

        _evolutionAffectGainQuery = GetEntityQuery<MCXenoEvolutionAffectGainComponent>();
        _hiveMemberQuery = GetEntityQuery<HiveMemberComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        FixNewlyEvolved();

        if (_net.IsClient)
            return;

        ProcessEvolutionPoints();
    }

    public bool CanEvolve(Entity<XenoEvolutionComponent> xeno, EntProtoId target, bool doPopup = true)
    {
        if (!_prototype.TryIndex(target, out var targetPrototype))
            return false;

        if (!TryGetHive(xeno, out var hive, doPopup))
            return false;

        if (!CheckBlockedRequirement(hive, target, targetPrototype, doPopup))
            return false;

        if (!CheckLivingRequirement(hive, target, targetPrototype, doPopup))
            return false;

        if (!CheckLeaderRequirement(hive, targetPrototype, doPopup))
            return false;

        return true;
    }

    private bool TryGetHive(
        Entity<XenoEvolutionComponent> xeno,
        out Entity<MCXenoHiveComponent> hive,
        bool doPopup)
    {
        hive = default;

        if (_mcXenoHive.GetHive(xeno.Owner) is not { } foundHive)
        {
            if (doPopup)
                _popup.PopupEntity(Loc.GetString("mc-xeno-evolution-failed-no-hive"), xeno, xeno, PopupType.MediumCaution);

            return false;
        }

        hive = foundHive;
        return true;
    }


    private bool CheckLeaderRequirement(
        Entity<MCXenoHiveComponent> hive,
        EntityPrototype targetPrototype,
        bool doPopup)
    {
        if (hive.Comp.Configuration.Evolution.WithoutRuler)
            return true;

        var hiveHasLeader = _mcXenoHive.HasRuler((hive, hive.Comp));
        if (hiveHasLeader)
            return true;

        var targetIsLeader = targetPrototype.HasComponent<MCXenoHiveLeaderComponent>();
        var canEvolveWithoutLeader = targetPrototype.TryGetComponent<XenoEvolutionComponent>(out var evo, _compFactory) && evo.CanEvolveWithoutGranter;

        if (targetIsLeader || canEvolveWithoutLeader)
            return true;

        if (doPopup)
            _popup.PopupEntity(Loc.GetString("mc-xeno-evolution-no-hive-leader"), hive, hive, PopupType.MediumCaution);

        return false;
    }

    private bool CheckLivingRequirement(
        Entity<MCXenoHiveComponent> hive,
        EntProtoId target,
        EntityPrototype targetPrototype,
        bool doPopup)
    {
        if (!hive.Comp.Configuration.Evolution.RequiredCasteCount.TryGetValue(target, out var required))
            return true;

        var living = _mcXenoHive.GetLiving(hive);
        if (living >= required)
            return true;

        if (doPopup)
            _popup.PopupEntity(Loc.GetString("mc-xeno-evolution-not-enough-quantity", ("prototype", targetPrototype.Name), ("count", required - living)), hive, hive, PopupType.MediumCaution);

        return false;
    }

    private bool CheckBlockedRequirement(
        Entity<MCXenoHiveComponent> hive,
        EntProtoId target,
        EntityPrototype targetPrototype,
        bool doPopup)
    {
        if (!hive.Comp.Configuration.Evolution.BlockedCastes.Contains(target))
            return true;

        if (doPopup)
            _popup.PopupEntity(Loc.GetString("mc-xeno-evolution-caste-blocked", ("prototype", targetPrototype.Name)), hive, hive, PopupType.MediumCaution);

        return false;
    }
}
