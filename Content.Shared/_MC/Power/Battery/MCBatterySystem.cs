using Content.Shared._MC.Power.Battery.Components;
using Content.Shared._MC.Power.Battery.Events;
using Content.Shared.Examine;
using JetBrains.Annotations;

namespace Content.Shared._MC.Power.Battery;

public sealed class MCBatterySystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<MCBatteryComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCBatteryComponent, ExaminedEvent>(OnExamined);
    }

    private void OnStartup(Entity<MCBatteryComponent> entity, ref ComponentStartup args)
    {
        var ev = new MCBatteryChargeChangedEvent(entity.Comp.Charge, entity.Comp.Charge, entity.Comp.MaxCharge);
        RaiseLocalEvent(entity, ref ev);
    }

    private void OnExamined(Entity<MCBatteryComponent> entity, ref ExaminedEvent args)
    {
        var charge = entity.Comp.MaxCharge == 0 ? 0 : entity.Comp.Charge / entity.Comp.MaxCharge * 100;
        args.PushMarkup(Loc.GetString("power-cell-component-examine-details", ("currentCharge", $"{charge:F0}")));
    }

    [PublicAPI]
    public void SetCharge(Entity<MCBatteryComponent?> entity, float value)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        var previousCharge = entity.Comp.Charge;

        entity.Comp.Charge = float.Clamp(value, 0, entity.Comp.MaxCharge);
        Dirty(entity);

        var ev = new MCBatteryChargeChangedEvent(previousCharge, entity.Comp.Charge, entity.Comp.MaxCharge);
        RaiseLocalEvent(entity, ref ev);
    }

    [PublicAPI]
    public float GetCharge(Entity<MCBatteryComponent?> entity) =>
        Resolve(entity, ref entity.Comp, false) ? entity.Comp.Charge : 0f;

    [PublicAPI]
    public float GetMaxCharge(Entity<MCBatteryComponent?> entity) =>
        Resolve(entity, ref entity.Comp, false) ? entity.Comp.MaxCharge : 0f;

    /// <summary>
    /// Fills the target battery using the charge available in the source battery.
    /// </summary>
    /// <returns>The amount transferred.</returns>
    public float TransferCharge(Entity<MCBatteryComponent> source, Entity<MCBatteryComponent> target)
    {
        if (source.Owner == target.Owner)
            return 0f;

        var amount = float.Min(source.Comp.Charge, target.Comp.MaxCharge - target.Comp.Charge);
        if (amount <= 0f)
            return 0f;

        SetCharge(source.AsNullable(), source.Comp.Charge - amount);
        SetCharge(target.AsNullable(), target.Comp.Charge + amount);
        return amount;
    }
}
