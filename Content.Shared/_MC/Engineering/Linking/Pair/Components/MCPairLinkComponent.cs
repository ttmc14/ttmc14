using Robust.Shared.GameStates;

namespace Content.Shared._MC.Engineering.Linking.Pair.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCPairLinkComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? LinkedEntityUid;
}
