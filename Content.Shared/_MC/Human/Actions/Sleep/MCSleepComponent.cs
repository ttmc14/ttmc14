using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Human.Actions.Sleep;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCSleepComponent : Component
{
    [DataField]
    public EntProtoId EffectId;

    [DataField, AutoNetworkedField]
    public EntProtoId ActionSleepId;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? ActionSleep;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? ActionWake;

    [DataField, AutoNetworkedField]
    public EntProtoId ActionWakeId;
}
