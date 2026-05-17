using Robust.Shared.GameStates;

namespace Content.Shared._MC.Weapons.Range.Delayed.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCWeaponRangeDelayedAlertComponent : Component
{
    [AutoNetworkedField]
    public TimeSpan TimeStart;

    [AutoNetworkedField]
    public TimeSpan TimeEnd;
}
