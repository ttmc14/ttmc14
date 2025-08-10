using Content.Shared._RMC14.Xenonids.Evolution;
using Content.Shared.Climbing.Components;
using Content.Shared.Climbing.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Evolution;

public sealed class MCXenoEvolutionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ClimbSystem _climb = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly XenoEvolutionSystem _evolution = default!;

    private readonly HashSet<EntityUid> _climbableTemp = new();
    private readonly HashSet<EntityUid> _intersectingTemp = new();

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

            var points = comp.PointsPerSecond;
            var gain =  points; // todo

            _evolution.SetPoints((uid, comp), FixedPoint2.Clamp(comp.Points + gain, 0, comp.Max));
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
