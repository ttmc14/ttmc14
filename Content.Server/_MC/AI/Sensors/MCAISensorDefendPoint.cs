using Content.Shared._MC.AI.Modules;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.AI.Sensors;

[DataDefinition]
public sealed partial class MCAISensorDefendPoint : MCAISensor<MCAISensorDefendPoint>
{
    public MCAISensorDefendPoint() { }

    [DataField("conditionKey")]
    public string ConditionKey { get; private set; } = "DefendPointExists";

    [DataField("outputTargetKey")]
    public string OutputTargetKey { get; private set; } = "DefendPointTarget";
}
