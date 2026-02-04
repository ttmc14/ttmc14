using Content.Shared._MC.Armor.Modules.Components;
using Content.Shared._MC.Armor.Modules.Events;
using Content.Shared._MC.Armor.Modules.Systems;
using Content.Shared.Actions;
using Content.Shared.Eye.Blinding.Components;

namespace Content.Shared._MC.Armor.EyeProtection;

public sealed class MCModuleEyeProtectionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = null!;
    [Dependency] private readonly MCArmorModuleRelaySystem _mcArmorModuleRelay = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCArmorModularClothingComponent, MCModuleEyeProtectionActionEvent>(_mcArmorModuleRelay.RelayEvent);

        SubscribeLocalEvent<MCModuleEyeProtectionComponent, MCArmorModuleRelayedEvent<GetItemActionsEvent>>(OnGetAction);
        SubscribeLocalEvent<MCModuleEyeProtectionComponent, MCArmorModuleRelayedEvent<MCModuleEyeProtectionActionEvent>>(OnAction);
        SubscribeLocalEvent<MCModuleEyeProtectionComponent, MCArmorModuleDetachedEvent>(OnDeattached);
    }

    private void OnGetAction(Entity<MCModuleEyeProtectionComponent> entity, ref MCArmorModuleRelayedEvent<GetItemActionsEvent> args)
    {
        args.Args.AddAction(ref entity.Comp.ActionUid, entity.Comp.ActionId);
        Dirty(entity);
    }

    private void OnAction(Entity<MCModuleEyeProtectionComponent> entity, ref MCArmorModuleRelayedEvent<MCModuleEyeProtectionActionEvent> args)
    {
        if (args.Args.Handled)
            return;

        args.Args.Handled = true;

        entity.Comp.Enabled = !entity.Comp.Enabled;
        Dirty(entity);

        _actions.SetToggled((args.Args.Action, args.Args.Action.Comp), entity.Comp.Enabled);

        var owner = Transform(entity).ParentUid;
        if (entity.Comp.Enabled)
        {
            EnsureComp<EyeProtectionComponent>(owner);
            return;
        }

        RemComp<EyeProtectionComponent>(owner);
    }

    private void OnDeattached(Entity<MCModuleEyeProtectionComponent> entity, ref MCArmorModuleDetachedEvent args)
    {
        RemComp<EyeProtectionComponent>(args.Armor);
    }
}
