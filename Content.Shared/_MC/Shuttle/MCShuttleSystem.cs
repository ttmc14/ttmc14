using Content.Shared._MC.FTL.Events;
using Content.Shared._MC.Operation;
using Content.Shared._MC.Shuttle.Events;
using Content.Shared._RMC14.Rules;

namespace Content.Shared._MC.Shuttle;

public sealed class MCShuttleSystem : EntitySystem
{
    public void Evacuate(Entity<MCShuttleComponent> entity)
    {
        if (!OnPlanet(entity) || entity.Comp.Evacuated)
            return;

        entity.Comp.Evacuated = true;
        Dirty(entity);

        var ev = new MCShuttleEvacuationEvent();
        RaiseLocalEvent(ev);
    }

    private bool OnPlanet(Entity<MCShuttleComponent> entity)
    {
        return HasComp<RMCPlanetComponent>(Transform(entity).MapUid);
    }
}
