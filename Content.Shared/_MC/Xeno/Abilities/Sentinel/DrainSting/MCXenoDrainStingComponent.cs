using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Sentinel.DrainSting;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoDrainStingComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Damage = 1;

    [DataField, AutoNetworkedField]
    public float PotencyMultiplier = 6;
}
