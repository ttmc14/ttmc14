using Content.Server.Atmos.EntitySystems;
using Content.Shared._MC.Flammable;
using Content.Shared.Atmos.Components;

namespace Content.Server._MC.Flammable;

public sealed class MCFlammableSystem : MCSharedFlammableSystem
{
    [Dependency] private readonly FlammableSystem _flammable = null!;

    public override bool OnFire(EntityUid uid)
    {
        return TryComp<FlammableComponent>(uid, out var flammableComponent) && flammableComponent.FireStacks > 0;
    }

    public override void AdjustFireStacks(EntityUid uid, float stacks, bool ignite = false)
    {
        base.AdjustFireStacks(uid, stacks, ignite);

        AdjustFireStacks(uid, 20, 30, stacks, ignite);
    }

    public override void AdjustFireStacks(EntityUid uid, int intensity, int duration, float stacks, bool ignite = false)
    {
        base.AdjustFireStacks(uid, stacks, ignite);

        if (!TryComp<FlammableComponent>(uid, out var component))
            return;

        component.Intensity = intensity;
        component.Duration = duration;

        Dirty(uid, component);

        _flammable.AdjustFireStacks(uid, stacks, ignite: ignite);
    }
}
