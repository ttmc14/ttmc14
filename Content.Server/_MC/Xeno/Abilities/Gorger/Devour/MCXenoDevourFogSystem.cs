using Content.Server._RMC14.Fog;
using Content.Shared._MC.Xeno.Abilities.Gorger.Devour.Components;
using Content.Shared._MC.Xeno.Abilities.Gorger.Devour.Events;
using Content.Shared.DoAfter;

namespace Content.Server._MC.Xeno.Abilities.Gorger.Devour;

public sealed class MCXenoDevourFogSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoDevourComponent, DoAfterAttemptEvent<MCXenoDevourDoAfterEvent>>(OnAttempt);
    }

    private void OnAttempt(Entity<MCXenoDevourComponent> entity, ref DoAfterAttemptEvent<MCXenoDevourDoAfterEvent> args)
    {
        var coords = Transform(entity).Coordinates;
        if (_lookup.GetEntitiesInRange<FogComponent>(coords, entity.Comp.Range, LookupFlags.Static).Count > 0)
        {
            args.Cancel();
        }
    }
}
