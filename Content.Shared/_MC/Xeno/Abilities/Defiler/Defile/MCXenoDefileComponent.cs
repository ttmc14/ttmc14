using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.Defile;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoDefileComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Solution = "chemicals";
}
