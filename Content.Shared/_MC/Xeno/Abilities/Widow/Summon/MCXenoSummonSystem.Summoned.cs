using Content.Shared.Mobs;
using Robust.Shared.Physics.Events;

namespace Content.Shared._MC.Xeno.Abilities.Widow.Summon;

public sealed partial class MCXenoSummonSystem
{
    private void OnSummonedShutdown(Entity<MCXenoSummonedComponent> entity, ref ComponentShutdown args)
    {
        SummonRemove(entity);
    }

    private void OnSummonedMobStateChanged(Entity<MCXenoSummonedComponent> entity, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        SummonRemove(entity);
    }

    private void OnSummonedPreventCollide(Entity<MCXenoSummonedComponent> entity, ref PreventCollideEvent args)
    {
        if (!IsXeno(args.OtherEntity))
            return;

        args.Cancelled = true;
    }

    private void SummonRemove(Entity<MCXenoSummonedComponent> entity)
    {
        if (!TryComp<MCXenoSummonComponent>(entity.Comp.OwnerUid, out var spawnerComponent))
            return;

        spawnerComponent.SummonUids.Remove(entity);
        Dirty(entity.Comp.OwnerUid, spawnerComponent);
    }
}
