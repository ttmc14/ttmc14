using Content.Shared.Inventory;
using Content.Shared.Weapons.Ranged;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Marine.Equipment.Weapon.Ranged;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCBackpackAmmoProviderComponent : Component, IShootable
{
    [DataField, AutoNetworkedField]
    public SlotFlags Slot = SlotFlags.BACK;
}
