using Content.Shared._MC.Armor.Modules;
using Content.Shared._MC.Armor.Modules.Components;
using Content.Shared._MC.Armor.Modules.Events;
using Content.Shared._MC.Armor.Modules.Systems;
using Content.Shared._MC.Armor.Vali.Events;
using Content.Shared._MC.Damage;
using Content.Shared._MC.Weapon.Vali.Components;
using Content.Shared._MC.Weapon.Vali.Events;
using Content.Shared._RMC14.Actions;
using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Popups;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Armor.Vali;

public sealed partial class MCModuleValiSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly SharedActionsSystem _actions = null!;
    [Dependency] private readonly SharedHandsSystem _hands = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;

    [Dependency] private readonly SharedRMCActionsSystem _rmcActions = null!;

    [Dependency] private readonly MCArmorModuleSystem _mcArmorModule = null!;
    [Dependency] private readonly MCArmorModuleRelaySystem _mcArmorModuleRelay = null!;
    [Dependency] private readonly MCDamageableSystem _mcDamageable = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCArmorModularClothingComponent, MCModuleValiConnectActionEvent>(_mcArmorModuleRelay.RelayEvent);
        SubscribeLocalEvent<MCArmorModularClothingComponent, MCModuleValiBoostActionEvent>(_mcArmorModuleRelay.RelayEvent);

        SubscribeLocalEvent<MCModuleValiComponent, MCArmorModuleRelayedEvent<ExaminedEvent>>(OnExamined);
        SubscribeLocalEvent<MCModuleValiComponent, MCArmorModuleRelayedEvent<MCModuleValiConnectActionEvent>>(OnActionConnect);
        SubscribeLocalEvent<MCModuleValiComponent, MCArmorModuleRelayedEvent<MCModuleValiBoostActionEvent>>(OnActionBoost);

        SubscribeLocalEvent<MCModuleValiComponent, MCArmorModuleRelayedEvent<GetItemActionsEvent>>(OnGetAction);
        SubscribeLocalEvent<MCModuleValiComponent, MCArmorModuleRelayedEvent<MCWeaponValiMeleeHitEvent>>(OnMeleeHit);

        SubscribeLocalEvent<MCModuleValiComponent, MCArmorModuleDetachedEvent>(OnDeattached);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCModuleValiComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.Boosted)
                continue;

            UpdateBoostedState((uid, component));
        }
    }

    private void OnExamined(Entity<MCModuleValiComponent> entity, ref MCArmorModuleRelayedEvent<ExaminedEvent> args)
    {
        using (args.Args.PushGroup(nameof(MCModuleValiComponent)))
        {
            args.Args.PushMarkup(Loc.GetString("mc-module-vali-examine-resources", ("value", entity.Comp.Resource), ("max", entity.Comp.ResourceMax)));
        }
    }

    private void OnActionConnect(Entity<MCModuleValiComponent> entity, ref MCArmorModuleRelayedEvent<MCModuleValiConnectActionEvent> relayedArgs)
    {
        var args = relayedArgs.Args;
        if (args.Handled)
            return;

        args.Handled = true;

        if (TryDeattachWeapon(entity))
            return;

        if (_mcArmorModule.GetUser(entity) is not { } userUid)
            return;

        if (!_hands.TryGetActiveItem(userUid, out var activeItemUid))
            return;

        if (!TryComp<MCWeaponValiComponent>(activeItemUid, out var valiComponent))
            return;

        entity.Comp.ConnectedWeaponUid = activeItemUid;
        entity.Comp.ConnectedWeaponHarvestAmount = valiComponent.HarvestAmount;
        Dirty(entity);

        EnsureComp<UnremoveableComponent>(activeItemUid.Value);

        _actions.SetToggled((args.Action, args.Action.Comp), true);
    }

    private void OnActionBoost(Entity<MCModuleValiComponent> entity, ref MCArmorModuleRelayedEvent<MCModuleValiBoostActionEvent> relayedArgs)
    {
        var args = relayedArgs.Args;
        if (args.Handled)
            return;

        args.Handled = true;

        if (entity.Comp.Boosted)
        {
            BoostOff(entity);
            return;
        }

        BoostOn(entity);
    }

    private void ActionSetToggled<T>(EntityUid uid, bool toggled) where T : BaseActionEvent
    {
        foreach (var action in _rmcActions.GetActionsWithEvent<T>(uid))
        {
            _actions.SetToggled((action, action), toggled);
            break;
        }
    }
}
