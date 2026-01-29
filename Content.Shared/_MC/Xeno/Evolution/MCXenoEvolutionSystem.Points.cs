using Content.Shared._MC.Xeno.Evolution.Components;
using Content.Shared._RMC14.Xenonids.Evolution;
using Content.Shared.FixedPoint;

namespace Content.Shared._MC.Xeno.Evolution;

public sealed partial class MCXenoEvolutionSystem
{
    private void ProcessEvolutionPoints()
    {
        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<XenoEvolutionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Max == FixedPoint2.Zero)
                continue;

            if (time < comp.LastPointsAt + EvolutionTickInterval)
                continue;

            comp.LastPointsAt = time;
            Dirty(uid, comp);

            var gain = CalculateEvolutionGain(uid, comp);
            _rmcEvolution.SetPoints((uid, comp), FixedPoint2.Clamp(comp.Points + gain, 0, comp.Max));
        }
    }

    private FixedPoint2 CalculateEvolutionGain(EntityUid uid, XenoEvolutionComponent comp)
    {
        GetEvolutionGainAffect(out var additional, out var multiplier);
        return (comp.PointsPerSecond + additional) * multiplier;
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
}
