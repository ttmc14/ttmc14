using Content.Shared.Radio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Engineering.Sentries.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCSentryComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool RadialMode;

    [DataField, AutoNetworkedField]
    public TimeSpan AlertDamageNextTime;

    [DataField, AutoNetworkedField]
    public TimeSpan AlertDamageDelay = TimeSpan.FromSeconds(4);

    [DataField, AutoNetworkedField]
    public TimeSpan AlertNextTime;

    [DataField, AutoNetworkedField]
    public TimeSpan AlertDelay = TimeSpan.FromSeconds(20);

    [DataField, AutoNetworkedField]
    public ProtoId<RadioChannelPrototype> AlertChannel = "MarineCommon";

    [DataField, AutoNetworkedField]
    public float DefenseCheckRange = 2.5f;

    [DataField, AutoNetworkedField]
    public bool AlertMode = true;
}
