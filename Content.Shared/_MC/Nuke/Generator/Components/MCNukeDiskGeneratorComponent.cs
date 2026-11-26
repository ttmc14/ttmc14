using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._MC.Nuke.Generator.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MCNukeDiskGeneratorComponent : Component
{
    [DataField(required: true)]
    public EntProtoId SpawnId;

    [DataField, AutoNetworkedField]
    public Color Color = Color.Red;

    [DataField]
    public TimeSpan InteractionTime = TimeSpan.FromSeconds(15);

    [DataField]
    public TimeSpan PrintTime = TimeSpan.FromSeconds(15);

    [DataField]
    public TimeSpan InterruptTime = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public FixedPoint2 CheckpointProgress;

    [DataField, AutoNetworkedField]
    public FixedPoint2 StepSize = 0.2;

    [DataField, AutoNetworkedField]
    public TimeSpan StepDuration = TimeSpan.FromSeconds(90);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan? StepEndsAt;

    [DataField]
    public DoAfterId? Operation;

    [AutoNetworkedField]
    public bool Busy;

    [DataField, AutoNetworkedField]
    public bool Disabled;

    [DataField]
    public SoundSpecifier CompletionSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");
}
