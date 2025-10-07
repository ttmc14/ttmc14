using Content.Server.Actions;
using Content.Server.Chat.Systems;
using Content.Shared.Inventory;
using Content.Shared.Whitelist;
using Content.Shared._RMC14.Marines.Announce;
using Content.Shared._MC.Actions.Orders;
using Robust.Shared.Random;

namespace Content.Server._MC.MarineOrders;

public sealed class MCMarineOrdersSystem : MCSharedSendOrdersSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MCSendOrdersComponent, MapInitEvent>(OnOrdersMapInit);
        SubscribeLocalEvent<MCSendOrdersComponent, ComponentShutdown>(OnOrdersShutdown);
    }

    private void OnOrdersMapInit(Entity<MCSendOrdersComponent> entity, ref MapInitEvent ev)
    {
        var comp = entity.Comp;
        _actions.AddAction(entity, ref comp.AttackActionEntity, comp.AttackAction);
        _actions.SetUseDelay(comp.AttackActionEntity, comp.Cooldown);

        _actions.AddAction(entity, ref comp.DefendActionEntity, comp.DefendAction);
        _actions.SetUseDelay(comp.DefendActionEntity, comp.Cooldown);

        _actions.AddAction(entity, ref comp.RetreatActionEntity, comp.RetreatAction);
        _actions.SetUseDelay(comp.RetreatActionEntity, comp.Cooldown);

        _actions.AddAction(entity, ref comp.RallyActionEntity, comp.RallyAction);
        _actions.SetUseDelay(comp.RallyActionEntity, comp.Cooldown);
    }

    private void OnOrdersShutdown(Entity<MCSendOrdersComponent> entity, ref ComponentShutdown ev)
    {
        _actions.RemoveAction(entity.Owner, entity.Comp.AttackActionEntity);
        _actions.RemoveAction(entity.Owner, entity.Comp.DefendActionEntity);
        _actions.RemoveAction(entity.Owner, entity.Comp.RetreatActionEntity);
        _actions.RemoveAction(entity.Owner, entity.Comp.RallyActionEntity);
    }
}
