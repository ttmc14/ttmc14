using Content.Shared._MC.Power.Battery.Components;
using Content.Shared._MC.Power.Battery.Events;

namespace Content.Shared._MC.Power.Battery;

public sealed class MCBatteryVisualsSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;

    /// <summary>
    /// Registers charge indicator updates for batteries with visual levels.
    /// </summary>
    public override void Initialize() =>
        SubscribeLocalEvent<MCBatteryVisualsComponent, MCBatteryChargeChangedEvent>(OnChargeChanged);

    private void OnChargeChanged(Entity<MCBatteryVisualsComponent> entity, ref MCBatteryChargeChangedEvent args) =>
        _appearance.SetData(entity, MCBatteryVisuals.Layer,  int.Max((int) float.Ceiling(args.Ration * entity.Comp.Levels), 1));
}
