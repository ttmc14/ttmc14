using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Weapon.Vali.Ui;

[Serializable, NetSerializable]
public sealed class MCWeaponValiSelectReagentMessage : BoundUserInterfaceMessage
{
    public readonly ProtoId<ReagentPrototype>? ReagentId;

    public MCWeaponValiSelectReagentMessage(ProtoId<ReagentPrototype>? reagentId)
    {
        ReagentId = reagentId;
    }
}
