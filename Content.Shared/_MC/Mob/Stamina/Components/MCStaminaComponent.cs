using Content.Shared.Alert;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Mob.Stamina.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCStaminaComponent : Component
{
    [DataField, AutoNetworkedField]
    public float StaminaMax = 200f;

    [DataField, AutoNetworkedField]
    public float Stamina = 200f;

    [DataField, AutoNetworkedField]
    public float ExhaustionThreshold = 50f;

    [DataField, AutoNetworkedField]
    public float RegenMultiplier = 1f;

    /// <summary>
    /// Thresholds for displaying the alt icon (fatigue levels).
    /// </summary>
    [DataField, AutoNetworkedField]
    public float[] TierThresholds = [200, 150, 100, 50, 0];

    /// <summary>
    /// Delay before regeneration begins after stamina has been depleted.
    /// Analogous to ‘RestPeriod’ in DM.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan RegenDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// The time when the next regeneration will begin.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan NextRegenTime;

    [DataField, AutoNetworkedField]
    public TimeSpan ExhaustionCooldown = TimeSpan.FromSeconds(10);

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            { "MCOxygen", 0.5f },
        },
    };

    [DataField, AutoNetworkedField]
    public TimeSpan LastExhaustionTime;

    [DataField]
    public ProtoId<AlertPrototype> StaminaAlert = "RMCStamina";

}
