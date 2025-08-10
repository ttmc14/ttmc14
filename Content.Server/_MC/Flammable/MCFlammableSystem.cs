using Content.Server.Atmos.EntitySystems;
using Content.Shared._MC.Flammable;
using Content.Shared.Atmos.Components;

namespace Content.Server._MC.Flammable;

public sealed class MCFlammableSystem : MCSharedFlammableSystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;

    public override bool OnFire(EntityUid uid)
    {
        return TryComp<FlammableComponent>(uid, out var flammableComponent) && flammableComponent.FireStacks > 0;
    }

    public override void RemoveStacks(EntityUid uid, float stacks)
    {
        base.RemoveStacks(uid, stacks);
        _flammable.AdjustFireStacks(uid, -stacks);
    }
}
