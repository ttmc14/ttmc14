using Robust.Shared.GameStates;

namespace Content.Shared._MC.Engineering.Sentries.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCSentryComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan AlertDamageNextTime;

    [DataField, AutoNetworkedField]
    public TimeSpan AlertDamageDelay = TimeSpan.FromSeconds(4);

    [DataField, AutoNetworkedField]
    public TimeSpan AlertNextTime;

    [DataField, AutoNetworkedField]
    public TimeSpan AlertDelay = TimeSpan.FromSeconds(20);

    [DataField, AutoNetworkedField]
    public float DefenseCheckRange = 2.5f;
}
