using Content.Shared._RMC14.Marines.Skills;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Research.Tools.Excavator;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCToolExcavationComponent : Component
{
    [DataField]
    public EntProtoId<SkillDefinitionComponent> SkillId = "MCSkillMedical";

    [DataField]
    public int SkillLevel = 4;

    [DataField]
    public TimeSpan ExcavateTime = TimeSpan.FromSeconds(10);

    [DataField]
    public float SearchRadius = 2f;
}
