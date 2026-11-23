using Content.Shared._MC.Aura;
using Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Components;
using Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Events;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Systems.Effects;

public sealed class MCXenoRageAuraSystem : EntitySystem
{
    private const string AuraLayer = "rage";

    [Dependency] private readonly MCAuraSystem _aura = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoRageAuraComponent, MCXenoRageActivateEvent>(OnActivate);
        SubscribeLocalEvent<MCXenoRageAuraComponent, MCXenoRageDeactivateEvent>(OnDeactivate);
    }

    private void OnActivate(Entity<MCXenoRageAuraComponent> entity, ref MCXenoRageActivateEvent args)
    {
        _aura.Set(entity, AuraLayer, entity.Comp.Aura);
    }

    private void OnDeactivate(Entity<MCXenoRageAuraComponent> entity, ref MCXenoRageDeactivateEvent args)
    {
        _aura.Remove(entity, AuraLayer);
    }
}
