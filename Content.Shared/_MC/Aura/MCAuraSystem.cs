using Content.Shared._MC.Aura.Components;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Aura;

public sealed class MCAuraSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;

    /// <summary>
    /// Applies or refreshes the named layer for this emitter.
    /// </summary>
    public void Set(EntityUid uid, string layer, ProtoId<MCAuraPrototype> preset, float strength = 1, float bloodTint = 0, EntityUid? emitter = null)
    {
        if (_net.IsClient || TerminatingOrDeleted(uid))
            return;

        var source = new MCAuraSource(layer, GetNetEntity(emitter ?? uid));
        var effect = new MCAuraEffect(preset, strength, bloodTint);
        var comp = EnsureComp<MCAuraComponent>(uid);
        if (comp.Effects.TryGetValue(source, out var previous) && previous == effect)
            return;

        comp.Effects[source] = effect;
        Dirty(uid, comp);
    }

    /// <summary>
    /// Removes only this emitter's named layer. The last layer releases the aura component.
    /// </summary>
    public void Remove(EntityUid uid, string layer, EntityUid? emitter = null)
    {
        if (_net.IsClient || TerminatingOrDeleted(uid) || !TryComp<MCAuraComponent>(uid, out var comp))
            return;

        var source = new MCAuraSource(layer, GetNetEntity(emitter ?? uid));
        if (!comp.Effects.Remove(source))
            return;

        if (comp.Effects.Count == 0)
        {
            RemComp<MCAuraComponent>(uid);
            return;
        }

        Dirty(uid, comp);
    }
}
