using Content.Shared._MC.Serialization.Loadout.Data;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Engineering.Vending.UI.Messages;

[Serializable, NetSerializable]
public sealed class MCVendorQuickEquipVendMessage(MCLoadout loadout) : BoundUserInterfaceMessage
{
    public readonly MCLoadout Loadout = loadout;
}
