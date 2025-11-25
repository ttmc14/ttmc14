using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Stamina;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCStaminaComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Current = 100;

    [DataField, AutoNetworkedField]
    public float Min = -40;

    [DataField]
    public float Max = 100;

    [DataField]
    public int RegenPerTick = 6;

    [DataField, AutoNetworkedField]
    public int Level;

    [DataField, AutoNetworkedField]
    public float DamageThresholds = -25;

    [DataField, AutoNetworkedField]
    public float DamageMultiplier = 5;

    [DataField, AutoNetworkedField]
    public float[] TierThresholds = [100, 70, 50, 25, 0];

    [DataField]
    public TimeSpan TimeBetweenChecks = TimeSpan.FromSeconds(1);

    [DataField, AutoNetworkedField]
    public TimeSpan NextRegen;

    [DataField, AutoNetworkedField]
    public TimeSpan NextCheck;

    [DataField]
    public ProtoId<AlertPrototype> StaminaAlert = "RMCStamina";

    [DataField]
    public TimeSpan RestPeriod = TimeSpan.FromSeconds(3);

}
