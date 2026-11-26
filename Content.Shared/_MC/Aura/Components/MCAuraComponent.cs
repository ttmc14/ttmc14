using Robust.Shared.GameStates;

namespace Content.Shared._MC.Aura.Components;

/// <summary>
/// Holds the visual effects currently applied to an entity, keyed by their source.
/// Each source can refresh or remove its own effect.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
[Access(typeof(MCAuraSystem))]
public sealed partial class MCAuraComponent : Component
{
    /// <summary>
    /// Active outlines and sprite filters keyed by the layer name and emitter.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<MCAuraSource, MCAuraEffect> Effects = new();
}
