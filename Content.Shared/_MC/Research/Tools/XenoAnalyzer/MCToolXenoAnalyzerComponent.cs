using Content.Shared._RMC14.Marines.Skills;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Research.Tools.XenoAnalyzer;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCToolXenoAnalyzerComponent : Component
{
    [DataField]
    public EntProtoId<SkillDefinitionComponent> SkillId = "MCSkillMedical";

    [DataField]
    public int SkillLevel = 4;

    [DataField]
    public TimeSpan BaseAnalyzeTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan FailedAnalyzeTime = TimeSpan.FromSeconds(15);

    [DataField]
    public TimeSpan SkillTimeReduction = TimeSpan.FromSeconds(2);
}
