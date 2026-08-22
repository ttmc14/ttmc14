using Content.Shared._MC.Weapons.AttackModeSelection.Core.Events;
using Content.Shared.Weapons.Ranged.Components;

using SelectiveFireType = Content.Shared.Weapons.Ranged.Components.SelectiveFire;

namespace Content.Shared._MC.Weapons.Range.SelectiveFire;

public sealed class MCWeaponSelectiveFireSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<MCWeaponSelectiveFireComponent, MCAttackModeSelectionEvent>(OnSelected);
    }

    private void OnSelected(Entity<MCWeaponSelectiveFireComponent> entity, ref MCAttackModeSelectionEvent args)
    {
        if (!Enum.TryParse(args.Id, out SelectiveFireType type))
            return;

        if (!TryComp<GunComponent>(entity, out var component))
        {
            Log.Warning($"Hasn't {nameof(GunComponent)}");
            return;
        }

        component.SelectedMode = type;
        Dirty(entity);
    }
}
