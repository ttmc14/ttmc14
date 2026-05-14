namespace Content.Shared._MC.Xeno.Abilities.Widow.Order.Events;

[ByRefEvent]
public readonly struct MCXenoOrderRaise(string orderId)
{
    public readonly string OrderId = orderId;
}
