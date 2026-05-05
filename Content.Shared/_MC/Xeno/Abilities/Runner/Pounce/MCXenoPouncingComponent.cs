using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPouncingComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<EntityUid> Hit = new();

    [DataField, AutoNetworkedField]
    public TimeSpan End;

    [DataField, AutoNetworkedField]
    public MapCoordinates Origin;
}
