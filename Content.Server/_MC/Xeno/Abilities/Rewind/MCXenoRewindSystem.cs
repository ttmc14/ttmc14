using Content.Server.Atmos.EntitySystems;
using Content.Shared._MC.Xeno.Abilities.Wraith.Rewind;
using Content.Shared.Atmos.Components;

namespace Content.Server._MC.Xeno.Abilities.Rewind;

public sealed class MCXenoRewindSystem : MCSharedXenoRewindSystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;

    protected override void OnTagetShutdown(Entity<MCXenoRewindTargetComponent> entity, ref ComponentShutdown _)
    {
        base.OnTagetShutdown(entity, ref _);

        if (entity.Comp.Canceled)
            return;

        if (TryComp<FlammableComponent>(entity, out var flammableComponent))
            _flammable.SetFireStacks(entity, entity.Comp.FireStacks, flammableComponent, entity.Comp.FireStacks > 0);
    }
}
