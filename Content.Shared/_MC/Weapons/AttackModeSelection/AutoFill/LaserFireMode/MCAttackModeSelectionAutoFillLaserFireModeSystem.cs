using Content.Shared._MC.Weapon.Laser.Components;
using Content.Shared._MC.Weapons.AttackModeSelection.Core.Components;

namespace Content.Shared._MC.Weapons.AttackModeSelection.AutoFill.LaserFireMode;

public sealed class MCAttackModeSelectionAutoFillLaserFireModeSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<MCAttackModeSelectionAutoFillLaserFireModeComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<MCAttackModeSelectionAutoFillLaserFireModeComponent> entity, ref ComponentStartup args)
    {
        if (!TryComp<MCAttackModeSelectionComponent>(entity, out var component))
        {
            Log.Warning($"Hasn't {nameof(MCAttackModeSelectionComponent)}");
            return;
        }

        if (!TryComp<MCWeaponLaserComponent>(entity, out var laserComponent))
        {
            Log.Warning($"Hasn't {nameof(MCWeaponLaserComponent)}");
            return;
        }

        var value = new Dictionary<string, MCAttackModeSelectionEntry>();
        foreach (var (type, mode) in laserComponent.Modes)
        {
            if (mode.Icon is null)
                continue;

            value[type] = new MCAttackModeSelectionEntry
            {
                Icon = mode.Icon,
            };
        }

        if (value.Count <= 1)
            return;

        component.Modes = value;
    }
}
