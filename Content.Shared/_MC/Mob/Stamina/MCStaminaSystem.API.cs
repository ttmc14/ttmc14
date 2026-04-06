using Content.Shared._MC.Mob.Stamina.Components;

namespace Content.Shared._MC.Mob.Stamina;

public sealed partial class MCStaminaSystem
{
    public void ApplyDamage(Entity<MCStaminaComponent?> entity, float amount, bool updateTimer = true, bool forceTimer = false)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        var newStamina = entity.Comp.Stamina - amount;
        if (newStamina <= 0)
        {
            var damage = entity.Comp.Damage * float.Abs(newStamina);
            _damageable.TryChangeDamage(entity, damage, ignoreResistances: true, interruptsDoAfters: false);
        }

        entity.Comp.Stamina = float.Clamp(newStamina, 0, entity.Comp.StaminaMax);

        if (amount > 0 && updateTimer || forceTimer)
            UpdateTimer(entity);

        CheckExhaustion((entity.Owner, entity.Comp));
        UpdateStaminaAlert((entity.Owner, entity.Comp));
    }

    public void UpdateTimer(Entity<MCStaminaComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        entity.Comp.NextRegenTime = _timing.CurTime + entity.Comp.RegenDelay;
    }

    public float GetLoss(Entity<MCStaminaComponent?> entity)
    {
        return Resolve(entity, ref entity.Comp, false)
            ? entity.Comp.StaminaMax - entity.Comp.Stamina
            : 0;
    }
}
