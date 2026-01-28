using Content.Shared._MC.ASRS.Components;

namespace Content.Shared._MC.ASRS.Systems;

public sealed class MCASRSCostSystem : EntitySystem
{
    private EntityQuery<MCASRSCostComponent> _costQuery;

    public override void Initialize()
    {
        base.Initialize();

        _costQuery = GetEntityQuery<MCASRSCostComponent>();
    }

    public int GetCost(EntityUid uid)
    {
        return !_costQuery.TryComp(uid, out var component) ? 0 : component.Cost;
    }
}
