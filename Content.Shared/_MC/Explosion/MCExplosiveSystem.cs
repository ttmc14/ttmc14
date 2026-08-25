using Content.Shared.Explosion.Components;

namespace Content.Shared._MC.Explosion;


public sealed class MCExplosiveSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCExplosiveComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<MCExplosiveComponent> ent, ref ComponentStartup args)
    {
        var comp = ent.Comp;
        if (comp.Falloff <= 0)
        {
            Log.Error($"MCExplosiveComponent on entity {ent.Owner} has Falloff <= 0. Aborting calculation.");
            return;
        }

        var explosive = EnsureComp<ExplosiveComponent>(ent.Owner);
        explosive.MaxIntensity = comp.Power;
        explosive.IntensitySlope = comp.Falloff * 2f;

        var radius = comp.Power / comp.Falloff;
        explosive.TotalIntensity = radius * radius * comp.Power * 1.2f;

        if (comp.ExplosionType is not null)
            explosive.ExplosionType = comp.ExplosionType.Value;

        explosive.CanCreateVacuum = false;
    }
}
