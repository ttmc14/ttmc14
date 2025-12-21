using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector.UI;

[Serializable, NetSerializable]
public sealed class MCXenoReagentSelectorBuiMsg : BoundUserInterfaceMessage
{
    public readonly string Id;

    public MCXenoReagentSelectorBuiMsg(string id)
    {
        Id = id;
    }
}
