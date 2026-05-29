using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Warrior.Momentum;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoMomentumComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Stacks;

    [DataField, AutoNetworkedField]
    public int StacksMax = 8;

    [DataField, AutoNetworkedField]
    public TimeSpan StacksDrainNext;

    [DataField, AutoNetworkedField]
    public TimeSpan StacksDrainDuration = TimeSpan.FromSeconds(3.5f);

    [DataField]
    public int StacksGainSlash = 1;

    [DataField]
    public float StacksDamageBonus = 2.5f;

    [DataField]
    public float StacksSpeedBonus = 0.05f;

    [DataField]
    public float StacksArmorBonus = 2.5f;
}
