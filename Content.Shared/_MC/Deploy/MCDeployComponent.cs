using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Deploy;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCDeployComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? DeployFixture = "sentry";

    [DataField, AutoNetworkedField]
    public MCDeployState State = MCDeployState.Item;

    [DataField, AutoNetworkedField]
    public TimeSpan DeployTime = TimeSpan.FromSeconds(10);
}

[Serializable, NetSerializable]
public enum MCDeployState
{
    Item,
    Deployed,
}

[Serializable, NetSerializable]
public enum MCDeployLayers
{
    Layer,
}
