using Robust.Shared.GameStates;

namespace Content.Shared._MC.Medical.SkillInjectors.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCSkillInjectableComponent : Component
{
    [DataField, AutoNetworkedField]
    public int SlotsFilled;

    [DataField, AutoNetworkedField]
    public int SlotsMax = 2;
}
