using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Atmos.Components;
using Content.Shared.Mobs;
using Robust.Shared.Configuration;

namespace Content.Shared._MC.Xeno.Fire;

public sealed class MCFireSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = null!;

    private bool _enabled;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlammableComponent, MobStateChangedEvent>(OnMobStateChanged);

        _configuration.OnValueChanged(MCConfigVars.MCFireResistOnDeath, value => _enabled = value, invokeImmediately: true);
    }

    private void OnMobStateChanged(Entity<FlammableComponent> entity, ref MobStateChangedEvent args)
    {
        if (!_enabled)
            return;

        if (HasComp<XenoComponent>(entity))
            return;

        if (args.NewMobState == MobState.Dead)
        {
            entity.Comp.FireStacks = 0;
            Dirty(entity);

            EnsureComp<RMCImmuneToIgnitionComponent>(entity);
            EnsureComp<RMCImmuneToFireTileDamageComponent>(entity);
            return;
        }

        RemCompDeferred<RMCImmuneToIgnitionComponent>(entity);
        RemCompDeferred<RMCImmuneToFireTileDamageComponent>(entity);
    }
}
