using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Biomass;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoBiomassSystem))]
public sealed partial class MCXenoBiomassComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Amount;
}
