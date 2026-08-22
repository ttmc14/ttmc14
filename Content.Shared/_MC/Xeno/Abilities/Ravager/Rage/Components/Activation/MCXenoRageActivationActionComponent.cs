using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Components.Activation;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoRageActivationActionComponent : Component
{
    [DataField]
    public float MinHealthThreshold = 0.5f;

    [DataField]
    public float RagePowerMultiplier = 0.75f;

    [DataField]
    public float RageSuperRageThreshold = 0.5f;

    [DataField]
    public float SpeedModifier = 1.75f;

    [DataField]
    public TimeSpan RageDuration = TimeSpan.FromSeconds(10f);

    [ViewVariables]
    public TimeSpan RageTimeEnd;
}
