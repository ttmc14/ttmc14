using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._MC.Xeno.Abilities.Lunge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MCXenoLungeStunnedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId[] Effects = new EntProtoId[] {"Stun", "KnockedDown"};

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan ExpireAt;

    [DataField, AutoNetworkedField]
    public NetEntity? Stunner;
}
