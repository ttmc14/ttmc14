using Robust.Shared.GameStates;

namespace Content.Shared._MC.Mob.Stamina.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCStaminaActiveComponent : Component
{
    [DataField, AutoNetworkedField]
    public float BaseStepCost = 1.0f;

    [DataField, AutoNetworkedField]
    public float DrainModifier = 1.0f;

    [DataField, AutoNetworkedField]
    public float ReviveThreshold = 100f;
}
