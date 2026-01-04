using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Warrior.Agility;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoAgilitySystem))]
public sealed partial class MCXenoAgilityActiveComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public int ArmorFlat;

    [ViewVariables, AutoNetworkedField]
    public float SpeedModifier;
}
