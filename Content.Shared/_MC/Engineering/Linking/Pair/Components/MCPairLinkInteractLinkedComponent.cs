using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Engineering.Linking.Pair.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCPairLinkInteractLinkedComponent : Component
{
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();
}
