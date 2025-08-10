using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Agility;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoAgilityComponent : Component
{
    [DataField, AutoNetworkedField]
    public int ArmorFlat = -30;

    [DataField, AutoNetworkedField]
    public float SpeedModifier = 1.45f;
}
