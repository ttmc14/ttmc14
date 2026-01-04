using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._MC.Xeno.Abilities.Crusher.Charge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MCXenoChargeActiveComponent: Component
{
    [DataField, AutoNetworkedField]
    public float Distance;

    [DataField, AutoNetworkedField]
    public float Steps;

    [DataField, AutoNetworkedField]
    public int Stage;

    [DataField, AutoNetworkedField]
    public DirectionFlag Direction;

    [DataField, AutoNetworkedField]
    public float SoundSteps;

    [DataField, AutoNetworkedField]
    public DirectionFlag Deviated;

    [DataField, AutoNetworkedField]
    public float DeviatedDistance;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan LastMovedAt;

    [DataField, AutoNetworkedField]
    public Angle? LastRelativeRotation;
}
