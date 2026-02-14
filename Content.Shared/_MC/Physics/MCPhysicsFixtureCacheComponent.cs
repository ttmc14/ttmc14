using Robust.Shared.GameStates;
using Robust.Shared.Physics.Dynamics;

namespace Content.Shared._MC.Physics;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCPhysicsFixtureCacheComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public Fixture? CachedFixture;

    [ViewVariables, AutoNetworkedField]
    public bool Ready;
}
