using Robust.Shared.Analyzers;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Stamina;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCStaminaDamageOnHitComponent : Component
{
    [DataField]
    public float Damage;

    [DataField, AutoNetworkedField]
    public bool RequiresWield = false;
}
