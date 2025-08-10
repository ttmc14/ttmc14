using Content.Shared._MC.Xeno.Blessings.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Construction.Blessings.UI;

[Serializable, NetSerializable]
public sealed class MCXenoBlessingsChooseBuiMsg : BoundUserInterfaceMessage
{
    public readonly ProtoId<MCXenoBlessingsEntryPrototype> Id;

    public MCXenoBlessingsChooseBuiMsg(ProtoId<MCXenoBlessingsEntryPrototype> id)
    {
        Id = id;
    }
}
