using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._MC.Xeno.Heal;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class MCXenoHealComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan RegenerationTimeNext;

    [DataField, AutoNetworkedField]
    public TimeSpan RegenerationDelay = TimeSpan.FromSeconds(10);

    [DataField]
    public float RestingMultiplier = 1f;

    [DataField]
    public float StandMultiplier = 0.2f;

    [DataField, AutoNetworkedField]
    public float RegenerationRampAmount = 0.005f;

    [DataField, AutoNetworkedField]
    public float RegenerationPower;
}
