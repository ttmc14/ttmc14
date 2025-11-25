using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Psydrain;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPsydrainableComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Available = true;
}
