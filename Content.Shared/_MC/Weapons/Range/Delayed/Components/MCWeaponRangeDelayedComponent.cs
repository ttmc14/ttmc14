using Robust.Shared.GameStates;

namespace Content.Shared._MC.Weapons.Range.Delayed.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCWeaponRangeDelayedComponent : Component
{
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1);
}
