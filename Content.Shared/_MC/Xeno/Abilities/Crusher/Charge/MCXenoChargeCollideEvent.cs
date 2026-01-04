namespace Content.Shared._MC.Xeno.Abilities.Crusher.Charge;

[ByRefEvent]
public record struct MCXenoChargeCollideEvent(Entity<MCXenoChargeActiveComponent> Charger, bool Handled = false);
