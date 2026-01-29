using Content.Shared._RMC14.Xenonids.Evolution;
using Content.Shared.Climbing.Components;

namespace Content.Shared._MC.Xeno.Evolution;

public sealed partial class MCXenoEvolutionSystem
{
    private void FixNewlyEvolved()
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
