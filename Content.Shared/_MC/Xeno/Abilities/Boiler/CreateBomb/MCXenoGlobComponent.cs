using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.CreateBomb;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoGlobComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Value;

    [DataField, AutoNetworkedField]
    public int Max = 7;
}
