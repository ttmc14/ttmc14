using Robust.Shared.GameStates;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Engineering.Deploy.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCDeployComponent : Component
{
    /// <summary>
    /// Enables/disables the <see cref="PhysicsComponent.Hard"/> field (by <see cref="SharedPhysicsSystem.SetHard"/>)
    /// for the corresponding fixture in the <see cref="PhysicsComponent"/>,
    /// which effectively removes collision when the entity is an item
    /// and enables it when it is placed
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? DeployFixture = "sentry";

    /// <summary>
    /// The current state of the entity;
    /// this can be modified in the component to deploy or item
    /// the entity by default.
    /// </summary>
    [DataField, AutoNetworkedField]
    public MCDeployState State = MCDeployState.Item;

    /// <summary>
    /// The delay time required to manually install the entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan DeployTime = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Allows you to deploy an entity manually.
    /// </summary>
    /// <remarks>
    /// You can still deploy it another way.
    /// By other systems.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public bool DeployAllowed = true;

    /// <summary>
    /// Allows you to undeploy an entity manually.
    /// </summary>
    /// <remarks>
    /// You can still undeploy it another way.
    /// By other systems.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public bool UndeployAllowed = true;

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
