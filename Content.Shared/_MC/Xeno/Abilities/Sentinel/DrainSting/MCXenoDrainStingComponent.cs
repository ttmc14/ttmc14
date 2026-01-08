using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Sentinel.DrainSting;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoDrainStingSystem), Other = AccessPermissions.None)]
public sealed partial class MCXenoDrainStingComponent : Component
{
    /// <summary>
    /// Percentage of toxin stacks removed from target (0 – 1).
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ToxicDrainRatio = 0.7f;

    [DataField, AutoNetworkedField]
    public float PotencyMultiplier = 6;

    [DataField, AutoNetworkedField]
    public EntProtoId EffectId = "MCEffectXenoDrainSting";

    /// <summary>
    /// Dealt damage: BaseBurnDamage * drainPotency.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BurnDamageMultiplier = 0.2f;

    #region Region

    /// <summary>
    /// Minimum duration of paralyze.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ParalyzeMinSeconds = 0.1f;

    [DataField, AutoNetworkedField]
    public float ParalyzeThreshold = 10f;

    [DataField, AutoNetworkedField]
    public float ParalyzeMultiplier = 0.1f;

    #endregion

    #region Buff

    /// <summary>
    /// Stack threshold for enhanced effect.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int BuffStackMargin = 10;

    [DataField, AutoNetworkedField]
    public ProtoId<EmotePrototype> BuffTargetEmote = "Scream";

    [DataField, AutoNetworkedField]
    public TimeSpan BuffDuration = TimeSpan.FromSeconds(10);

    #endregion

    #region Regen

    /// <summary>
    /// Health regeneration multiplier.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HealthRegenMultiplier = 1.5f;

    /// <summary>
    /// Plasma recovery multiplier.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float PlasmaRegenMultiplier = 3.5f;

    #endregion
}
