using Robust.Shared.GameStates;

namespace Content.Shared._MC.Skills.Injectors.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCSkillInjectableComponent : Component
{
    [DataField, AutoNetworkedField]
    public int SlotsFilled;

    [DataField, AutoNetworkedField]
    public int SlotsMax = 2;
}
