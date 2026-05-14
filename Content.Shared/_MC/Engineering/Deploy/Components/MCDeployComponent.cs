using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Engineering.Deploy.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCDeployComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? DeployFixture = "sentry";

    [DataField, AutoNetworkedField]
    public MCDeployState State = MCDeployState.Item;

    [DataField, AutoNetworkedField]
    public TimeSpan DeployTime = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Entity prototype to use when spawning from the stack.
    /// If not specified, the current entity is used.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId? DeployedPrototype;
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
