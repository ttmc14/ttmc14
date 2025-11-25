using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Nuke.Generator.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCNukeDiskGeneratorComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId SpawnId = string.Empty;

    [DataField, AutoNetworkedField]
    public Color Color = Color.Red;

    [DataField, AutoNetworkedField]
    public TimeSpan InteractionTime = TimeSpan.FromSeconds(15);

    [DataField, AutoNetworkedField]
    public FixedPoint2 OverallProgress;

    [DataField, AutoNetworkedField]
    public FixedPoint2 CheckpointProgress;

    [DataField, AutoNetworkedField]
    public FixedPoint2 StepSize = 0.2;

    [DataField, AutoNetworkedField]
    public TimeSpan StepDuration = TimeSpan.FromSeconds(90);
}
