using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Rage;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoRageActiveComponent : Component
{
    [DataField, AutoNetworkedField]
    public float MinHealthThreshold = 0.75f;

    [DataField, AutoNetworkedField]
    public Color AuraColor = Color.Red;

    [DataField, AutoNetworkedField]
    public float AuraStrength = 3;

    [DataField, AutoNetworkedField]
    public float StaggerStunImmuneThreshold = 0.5f;

    [DataField, AutoNetworkedField]
    public float RagePower;

    [DataField, AutoNetworkedField]
    public bool OnCooldown;

    [DataField, AutoNetworkedField]
    public float SpeedModifier = 0.5f;

    [DataField, AutoNetworkedField]
    public float HealPerSlash = 20f;
}
