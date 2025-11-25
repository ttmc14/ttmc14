using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Systems;
using Content.Shared._RMC14.Xenonids.Evolution;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Climbing.Components;
using Content.Shared.Climbing.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Content.Shared.Prototypes;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Evolution;

public sealed class MCXenoEvolutionSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly ClimbSystem _climb = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    [Dependency] private readonly XenoEvolutionSystem _rmcEvolution = default!;
    [Dependency] private readonly SharedXenoHiveSystem _rmcXenoHive = default!;

    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = default!;

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

        FixEvolution();

        if (_net.IsClient)
            return;

        var time = _timing.CurTime;

        var evolution = EntityQueryEnumerator<XenoEvolutionComponent>();
        while (evolution.MoveNext(out var uid, out var comp))
        {
            if (comp.Max == FixedPoint2.Zero)
                continue;

            if (time < comp.LastPointsAt + TimeSpan.FromSeconds(1))
                continue;

            comp.LastPointsAt = time;
            Dirty(uid, comp);

            GetEvolutionGainAffect(out var gainAdditional, out var gainMultiplier);
            var points = comp.PointsPerSecond;
            var gain = (points + gainAdditional) * gainMultiplier;

            _rmcEvolution.SetPoints((uid, comp), FixedPoint2.Clamp(comp.Points + gain, 0, comp.Max));
        }
    }

    public bool CanEvolve(Entity<XenoEvolutionComponent> xeno, EntProtoId target, bool doPopup = true)
    {
        if (!_prototype.TryIndex(target, out var targetPrototype))
            return false;

        if (!_hiveMemberQuery.TryComp(xeno, out var hiveMemberComponent) || _rmcXenoHive.GetHive((xeno.Owner, hiveMemberComponent)) is not {} hive)
        {
            Popup(Loc.GetString("mc-xeno-evolution-failed-no-hive"));
            return false;
        }

        if (hive.Comp.CaseEvolutionBlock.Contains(target))
        {
            Popup(Loc.GetString("mc-xeno-evolution-not-available"));
            return false;
        }

        var living = _mcXenoHive.GetLiving(hive);
        if (hive.Comp.CasteEvolutionCountRequire.TryGetValue(target, out var countRequire) && _mcXenoHive.GetLiving(hive) < countRequire)
        {
            Popup(Loc.GetString("mc-xeno-evolution-not-enough-quantity", ("prototype", targetPrototype.Name), ("count", countRequire - living)));
            return false;
        }

        var hiveHasLeader = _mcXenoHive.HiveHasRuler((hive, hive));
        var targetLeader = targetPrototype.HasComponent<MCXenoHiveLeaderComponent>();
        var canEvolveWithoutLeader =
            targetPrototype.TryGetComponent<XenoEvolutionComponent>(out var evolutionComponent, _compFactory) &&
            evolutionComponent.CanEvolveWithoutGranter;

        if (!hive.Comp.CanEvolveWithoutRuler && !hiveHasLeader && !targetLeader && !canEvolveWithoutLeader)
        {
            Popup(Loc.GetString("mc-xeno-evolution-no-hive-leader"));
            return false;
        }

        return true;

        void Popup(string msg)
        {
            if (!doPopup)
                return;

            _popup.PopupEntity(msg, xeno, xeno, PopupType.MediumCaution);
        }
    }

    private void GetEvolutionGainAffect(out FixedPoint2 additional, out FixedPoint2 multiplier, EntityUid? hiveEnt = null)
    {
        additional = 0;
        multiplier = 1;

        var query = EntityQueryEnumerator<MCXenoEvolutionAffectGainComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (hiveEnt is not null && _hiveMemberQuery.TryComp(uid, out var hiveMemberComponent) && hiveMemberComponent.Hive != hiveEnt)
                continue;

            additional += component.Additional;
            multiplier += component.Multiplier;
        }
    }

    private void FixEvolution()
    {
        var newly = EntityQueryEnumerator<XenoNewlyEvolvedComponent>();
        while (newly.MoveNext(out var uid, out var comp))
        {
            if (comp.TriedClimb)
            {
                _intersectingTemp.Clear();
                _entityLookup.GetEntitiesIntersecting(uid, _intersectingTemp);
                for (var i = comp.StopCollide.Count - 1; i >= 0; i--)
                {
                    var colliding = comp.StopCollide[i];
                    if (!_intersectingTemp.Contains(colliding))
                        comp.StopCollide.RemoveAt(i);
                }

                if (comp.StopCollide.Count == 0)
                    RemCompDeferred<XenoNewlyEvolvedComponent>(uid);

                continue;
            }

            comp.TriedClimb = true;
            if (!TryComp<ClimbingComponent>(uid, out var climbing))
                continue;

            _climbableTemp.Clear();
            _entityLookup.GetEntitiesIntersecting(uid, _climbableTemp);

            foreach (var intersecting in _climbableTemp)
            {
                if (!HasComp<ClimbableComponent>(intersecting))
                    continue;

                _climb.ForciblySetClimbing(uid, intersecting);
                Dirty(uid, climbing);
                break;
            }
        }
    }
}
