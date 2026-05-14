using Content.Shared._MC.Xeno.Abilities.Widow.Order.Events;

namespace Content.Shared._MC.Xeno.Abilities.Widow.Order;

public sealed class MCXenoOrderSystem : MCXenoAbilitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoOrderComponent, MCXenoOrderActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoOrderComponent> entity, ref MCXenoOrderActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryUseAction(entity, args.Action))
            return;

        var ev = new MCXenoOrderRaise(args.OrderId);
        RaiseLocalEvent(entity, ref ev);

        args.Handled = true;
    }

    public void SetOrder(EntityUid originUid, EntityUid targetUid, string orderId)
    {
        if (TryComp<MCXenoOrderComponent>(originUid, out var originComponent) && originComponent.Order != orderId)
        {
            originComponent.Order = orderId;
            Dirty(originUid, originComponent);
        }

        var component = EnsureComp<MCXenoOrderReceiverComponent>(targetUid);
        if (component.CurrentOrder == orderId)
            return;

        component.CurrentOrder = orderId;
        Dirty(targetUid, component);
    }

    public void ReplyOrder(EntityUid originUid, EntityUid targetUid)
    {
        if (!TryComp<MCXenoOrderComponent>(originUid, out var originComponent))
            return;

        SetOrder(originUid, targetUid, originComponent.Order);
    }
}
