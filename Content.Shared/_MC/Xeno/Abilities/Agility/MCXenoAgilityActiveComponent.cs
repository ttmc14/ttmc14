using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Agility;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoAgilityActiveComponent : Component
{
    [DataField, AutoNetworkedField]
    public int ArmorFlat;

    [DataField, AutoNetworkedField]
    public float SpeedModifier;
}
