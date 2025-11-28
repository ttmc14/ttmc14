using Robust.Shared.GameStates;

namespace Content.Shared._MC.Aura;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCAuraComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<MCAuraId, MCAuraEntry> Entries = new();

    [DataField, AutoNetworkedField]
    public Dictionary<MCAuraId, TimeSpan> ExpiresAt = new();

    [DataField, AutoNetworkedField]
    public bool ComponentCleanable;

    [AutoNetworkedField]
    public List<MCAuraId> RemoveQueue = new();
}
