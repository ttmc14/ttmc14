using Robust.Shared.GameStates;

namespace Content.Shared._MC.Smoke.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCSmokePlasmaComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Amount;

    [DataField, AutoNetworkedField]
    public float Multiplier;
}
