using Content.Shared._MC.Power.Battery;
using Content.Shared._MC.Power.Battery.Components;
using Content.Shared._MC.Power.Powerpack.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;

namespace Content.Shared._MC.Power.Powerpack;

public sealed class MCPowerpackSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly MCBatterySystem _battery = null!;

    public override void Initialize() =>
        SubscribeLocalEvent<MCPowerpackComponent, InteractUsingEvent>(OnInteractUsing);

    private void OnInteractUsing(Entity<MCPowerpackComponent> entity, ref InteractUsingEvent args)
    {
        if (!TryComp<MCBatteryComponent>(args.Used, out var usedBattery))
            return;

        if (!TryComp<MCBatteryComponent>(entity, out var powerpackBattery))
            return;

        if (args.Used == entity.Owner)
            return;

        var powerpackCharge = _battery.GetCharge((entity.Owner, powerpackBattery));
        var batteryCharge = usedBattery.Charge;
        var batteryMaxCharge = usedBattery.MaxCharge;

        var spaceInBattery = batteryMaxCharge - batteryCharge;
        if (spaceInBattery <= 0f)
        {
            _popup.PopupClient(Loc.GetString("mc-powerpack-battery-full"), entity, args.User);
            return;
        }

        var transferAmount = float.Min(powerpackCharge, spaceInBattery);
        if (transferAmount <= 0f)
        {
            _popup.PopupClient(Loc.GetString("mc-powerpack-empty"), entity, args.User);
            return;
        }

        _battery.SetCharge((entity.Owner, powerpackBattery), powerpackCharge - transferAmount);
        _battery.SetCharge((args.Used, usedBattery), batteryCharge + transferAmount);

        _popup.PopupClient(
            Loc.GetString("mc-powerpack-charged", ("amount", transferAmount)),
            entity,
            args.User,
            PopupType.Medium);

        args.Handled = true;
    }
}
