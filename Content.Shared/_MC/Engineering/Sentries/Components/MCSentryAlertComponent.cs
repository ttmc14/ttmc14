using Content.Shared.Radio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Engineering.Sentries.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCSentryAlertComponent  : Component
{
    [DataField]
    public LocId DestroyedLoc = "mc-sentry-destroyed-alert";

    [DataField]
    public LocId DamageLoc = "mc-sentry-damage-alert";

    [DataField]
    public ProtoId<RadioChannelPrototype> AlertChannel = "MarineCommon";

    [DataField, AutoNetworkedField]
    public bool AlertMode = true;

    [DataField, AutoNetworkedField]
    public TimeSpan AlertDelay = TimeSpan.FromSeconds(4);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan AlertNext;
}
