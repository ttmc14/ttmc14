using Content.Shared.Radio;
using Content.Shared.Inventory;
using Content.Shared._RMC14.Marines.Squads;
using Content.Shared.Radio.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Actions.Orders;

public abstract class MCSharedSendOrdersSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCSendOrdersComponent, MCAttackSendOrdersActionEvent>(OnAction);
        SubscribeLocalEvent<MCSendOrdersComponent, MCDefendSendOrdersActionEvent>(OnAction);
        SubscribeLocalEvent<MCSendOrdersComponent, MCRetreatSendOrdersActionEvent>(OnAction);
        SubscribeLocalEvent<MCSendOrdersComponent, MCRallySendOrdersActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCSendOrdersComponent> entity, ref MCAttackSendOrdersActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        TrySendOrders(entity, entity.Comp.AttackOrderSays, entity.Comp.AttackEffectOnAction);
    }

    private void OnAction(Entity<MCSendOrdersComponent> entity, ref MCDefendSendOrdersActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        TrySendOrders(entity, entity.Comp.DefendOrderSays, entity.Comp.DefendEffectOnAction);
    }

    private void OnAction(Entity<MCSendOrdersComponent> entity, ref MCRetreatSendOrdersActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        TrySendOrders(entity, entity.Comp.RetreatOrderSays, entity.Comp.RetreatEffectOnAction);
    }

    private void OnAction(Entity<MCSendOrdersComponent> entity, ref MCRallySendOrdersActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        TrySendOrders(entity, entity.Comp.RallyOrderSays, entity.Comp.RallyEffectOnAction);
    }

    private void TrySendOrders(EntityUid entity, List<LocId> listOrdersSays, EntProtoId effectOnAction)
    {
        var random = new System.Random();
        var selectedMessage = listOrdersSays[random.Next(0, listOrdersSays.Count - 1)];

        Spawn(effectOnAction, Transform(entity).Coordinates);

        var headsetChannel = GetHeadset(entity);
        var message = Loc.GetString(selectedMessage);
        SendMessage(entity, message, headsetChannel);
    }

    private ProtoId<RadioChannelPrototype>? TryGetSquadRadioChannel(EntityUid entity)
    {
        if (!TryComp<SquadMemberComponent>(entity, out var squad))
            return null;

        if (!TryComp<SquadTeamComponent>(squad.Squad, out var team))
            return null;

        return team.Radio;
    }

    private ProtoId<RadioChannelPrototype>? GetHeadset(EntityUid entity)
    {
        var hasHeadset = false;
        var slots = _inventory.GetSlotEnumerator(entity);

        while (slots.MoveNext(out var slot))
        {
            if (slot.ContainedEntity is null)
                continue;

            if (slot.ID != "ears")
                continue;

            hasHeadset = true;
            break;
        }

        if (!hasHeadset)
            return null;

        var squadChannel = TryGetSquadRadioChannel(entity);
        if (squadChannel.HasValue && HasChannelInHeadset(entity, squadChannel.Value))
            return squadChannel.Value;

        if (TryComp<MCSendOrdersComponent>(entity, out var ordersComp))
            return ordersComp.DefaultFallbackChannel;

        return null;
    }

    private bool HasChannelInHeadset(EntityUid entity, ProtoId<RadioChannelPrototype> channel)
    {
        var slots = _inventory.GetSlotEnumerator(entity);
        while (slots.MoveNext(out var slot))
        {
            if (slot.ContainedEntity is not { } contained)
                continue;

            if (slot.ID != "ears")
                continue;

            if (TryComp<EncryptionKeyHolderComponent>(contained, out var keyHolder))
            {
                if (keyHolder.Channels.Contains(channel))
                {
                    return true;
                }
            }
        }

        return false;
    }

    protected virtual void SendMessage(EntityUid uid, string message, ProtoId<RadioChannelPrototype>? channel)
    {
    }
}
