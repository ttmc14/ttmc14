using Content.Shared._MC.Armor.Modules.Core;
using Content.Shared._MC.Armor.Modules.Core.Components;
using Content.Shared._MC.Armor.Modules.Core.Events;
using Content.Shared._MC.Armor.Modules.Features.HealthScan.Components;
using Content.Shared._MC.Armor.Modules.Features.HealthScan.Events;
using Content.Shared._RMC14.Medical.Scanner;
using Content.Shared.Actions;

namespace Content.Shared._MC.Armor.Modules.Features.HealthScan;

public sealed class MCModuleHealthScanSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = null!;
    [Dependency] private readonly HealthScannerSystem _rmcHealthScanner = null!;
    [Dependency] private readonly MCArmorModuleSharedSystem _mcArmorModuleShared = null!;
    [Dependency] private readonly MCArmorModuleRelaySystem _mcArmorModuleRelay = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCArmorModularClothingComponent, MCModuleHealthScanActionEvent>(_mcArmorModuleRelay.RelayEvent);

        SubscribeLocalEvent<MCModuleHealthScanComponent, MCArmorModuleRelayedEvent<GetItemActionsEvent>>(OnGetAction);
        SubscribeLocalEvent<MCModuleHealthScanComponent, MCArmorModuleRelayedEvent<MCModuleHealthScanActionEvent>>(OnActionMedicalScan);
    }

    private void OnGetAction(Entity<MCModuleHealthScanComponent> entity, ref MCArmorModuleRelayedEvent<GetItemActionsEvent> args)
    {
        args.Args.AddAction(ref entity.Comp.ActionUid, entity.Comp.ActionId);
        Dirty(entity);
    }

    private void OnActionMedicalScan(Entity<MCModuleHealthScanComponent> entity, ref MCArmorModuleRelayedEvent<Events.MCModuleHealthScanActionEvent> relayedArgs)
    {
        var args = relayedArgs.Args;
        if (args.Handled)
            return;

        args.Handled = true;

        if (_mcArmorModuleShared.GetUser(entity) is not { } userUid || !TryComp<HealthScannerComponent>(entity, out var healthScanner))
            return;

        healthScanner.Target = userUid;
        Dirty(entity,  healthScanner);

        _ui.TryOpenUi(entity.Owner, HealthScannerUIKey.Key, userUid);
        _rmcHealthScanner.UpdateUI((entity, healthScanner));
    }
}
