using Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Components;
using Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Events;
using Content.Shared._RMC14.Aura;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Systems.Effects;

public sealed class MCXenoRageAuraSystem : EntitySystem
{
    [Dependency] private readonly SharedAuraSystem _rmcAura = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoRageAuraComponent, MCXenoRageActivateEvent>(OnActivate);
        SubscribeLocalEvent<MCXenoRageAuraComponent, MCXenoRageDeactivateEvent>(OnDeactivate);
    }

    private void OnActivate(Entity<MCXenoRageAuraComponent> entity, ref MCXenoRageActivateEvent args)
    {
        _rmcAura.GiveAura(entity, entity.Comp.AuraColor, null, entity.Comp.AuraStrength);
    }

    private void OnDeactivate(Entity<MCXenoRageAuraComponent> entity, ref MCXenoRageDeactivateEvent args)
    {
        RemCompDeferred<AuraComponent>(entity);
    }
}
