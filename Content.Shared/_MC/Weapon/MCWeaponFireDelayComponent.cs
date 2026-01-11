using Content.Shared.DoAfter;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Weapon;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCWeaponFireDelayComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1);

    public DoAfterId? DoAfterId;
}
