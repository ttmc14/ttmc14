using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.StatusEffects.HealingInfusion;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoStatusEffectHealingInfusionComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan TickNext;

    [DataField, AutoNetworkedField]
    public TimeSpan TickDelay = TimeSpan.FromSeconds(1);

    [DataField, AutoNetworkedField]
    public TimeSpan HealthDurationRemaining = TimeSpan.FromSeconds(10);

    [DataField, AutoNetworkedField]
    public TimeSpan SunderDurationRemaining = TimeSpan.FromSeconds(10);

    [DataField]
    public float Heal = 6;

    [DataField]
    public Color EffectAuraColor = Color.FromHex("#DDFFD3");

    [DataField]
    public float EffectAuraStrength = 3;
}
