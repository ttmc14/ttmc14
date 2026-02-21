using Content.Shared.Chemistry.Reagent;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Weapon.Vali.Events.DoAfter;

[Serializable, NetSerializable]
public sealed partial class MCWeaponValiSelectReagentDoAfterEvent : SimpleDoAfterEvent
{
    public readonly ProtoId<ReagentPrototype> ReagentId;

    public MCWeaponValiSelectReagentDoAfterEvent(ProtoId<ReagentPrototype> reagentId)
    {
        ReagentId = reagentId;
    }
}

