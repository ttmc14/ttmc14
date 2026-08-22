using Content.Shared._MC.Weapons.AttackModeSelection.Core.Components;
using Content.Shared._RMC14.Weapons.Ranged;

using SelectiveFireType = Content.Shared.Weapons.Ranged.Components.SelectiveFire;

namespace Content.Shared._MC.Weapons.AttackModeSelection.AutoFill.SelectiveFire;

public sealed class MCAttackModeSelectionAutoFillSelectiveFireSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<MCAttackModeSelectionAutoFillSelectiveFireComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<MCAttackModeSelectionAutoFillSelectiveFireComponent> entity, ref ComponentStartup args)
    {
        if (!TryComp<MCAttackModeSelectionComponent>(entity, out var component))
        {
            Log.Warning($"Hasn't {nameof(MCAttackModeSelectionComponent)}");
            return;
        }

        if (!TryComp<RMCSelectiveFireComponent>(entity, out var fireComponent))
        {
            Log.Warning($"Hasn't {nameof(RMCSelectiveFireComponent)}");
            return;
        }

        var value = new Dictionary<string, MCAttackModeSelectionEntry>();

        TryAdd(SelectiveFireType.Burst);
        TryAdd(SelectiveFireType.FullAuto);
        TryAdd(SelectiveFireType.SemiAuto);

        if (value.Count <= 1)
            return;

        component.Modes = value;
        return;

        void TryAdd(SelectiveFireType type)
        {
            var modes = fireComponent.BaseFireModes;
            if (!modes.HasFlag(type))
                return;

            var name = Enum.GetName(type);
            if (name is null)
                return;

            value[name] = new MCAttackModeSelectionEntry
            {
                Icon = entity.Comp.Icons[type],
            };
        }
    }
}
