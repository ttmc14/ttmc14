using Robust.Shared.GameStates;

namespace Content.Shared._MC.Marine.Equipment.ThermalCloak;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCThermalCloakComponent : Component
{
    [DataField, AutoNetworkedField]
    public float StealthDelay = 3f;

    [DataField, AutoNetworkedField]
    public float WalkDrain = 1.5f;

    [DataField, AutoNetworkedField]
    public float RunDrain = 3f;

    [DataField, AutoNetworkedField]
    public float StillRecovery = 2f;

    [DataField, AutoNetworkedField]
    public float ShimmerAlpha = 0.5f;

    [DataField, AutoNetworkedField]
    public float StillAlpha = 0.05f;

    [DataField, AutoNetworkedField]
    public float WalkAlpha = 0.3f;

    [DataField, AutoNetworkedField]
    public float RunAlpha = 0.5f;

    [DataField, AutoNetworkedField]
    public float ForcedCooldown = 3f;

    [DataField, AutoNetworkedField]
    public EntityUid? Wearer;

    [DataField, AutoNetworkedField]
    public bool Enabled;

    [DataField, AutoNetworkedField]
    public float Energy = 100f;

    [DataField, AutoNetworkedField]
    public float EnergyMax = 100f;

    [DataField, AutoNetworkedField]
    public bool AllowMovement = true;

    [DataField, AutoNetworkedField]
    public bool AllowShooting = true;

    [DataField, AutoNetworkedField]
    public bool AllowMeleeWeapon = true;
}
