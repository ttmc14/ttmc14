using Content.Shared.Actions;

namespace Content.Shared._MC.Xeno.Abilities.Widow.Order;

public sealed partial class MCXenoOrderActionEvent : InstantActionEvent
{
    [DataField]
    public string OrderId = string.Empty;
}
