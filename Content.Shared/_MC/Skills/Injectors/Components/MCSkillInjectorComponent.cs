using Content.Shared._RMC14.Marines.Skills;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Skills.Injectors.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCSkillInjectorComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId<SkillDefinitionComponent> Skill = "";

    [DataField, AutoNetworkedField]
    public int Level = 1;

    [DataField, AutoNetworkedField]
    public int LevelMax = 1;
}
