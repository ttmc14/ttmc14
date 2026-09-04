using Content.Shared._RMC14.Marines.Skills;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Medical.SkillInjectors.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCSkillInjectorComponent : Component
{
    #region Requirements

    [DataField, AutoNetworkedField]
    public EntProtoId<SkillDefinitionComponent>? RequirementSkill;

    [DataField, AutoNetworkedField]
    public int? RequirementLevel;

    #endregion

    #region Gain

    [DataField, AutoNetworkedField]
    public EntProtoId<SkillDefinitionComponent> Skill = "";

    [DataField, AutoNetworkedField]
    public int Level = 1;

    [DataField, AutoNetworkedField]
    public int LevelMax = 1;

    #endregion
}
