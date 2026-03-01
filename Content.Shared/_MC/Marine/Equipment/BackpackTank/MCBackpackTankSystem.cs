using System.Diagnostics.CodeAnalysis;
using Content.Shared._RMC14.Weapons.Ranged.Flamer;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Inventory;
using Robust.Shared.Containers;

namespace Content.Shared._MC.Marine.Equipment.BackpackTank;

public sealed class MCBackpackTankSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = null!;
    [Dependency] private readonly InventorySystem _inventory = null!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = null!;

    public bool TryGetTankSolution(Entity<RMCFlamerAmmoProviderComponent> entity,
        [NotNullWhen(true)] out Entity<SolutionComponent>? solutionEntity,
        [NotNullWhen(true)] out Entity<RMCFlamerTankComponent>? tankEntity)
    {
        solutionEntity = null;
        tankEntity = null;

        if (!_container.TryGetContainingContainer((entity.Owner, null), out var holder))
            return false;

        var inventoryEnumerator = _inventory.GetSlotEnumerator(holder.Owner);
        while (inventoryEnumerator.MoveNext(out var slot))
        {
            if (slot.ContainedEntity is not { } uid)
                continue;

            if (!HasComp<MCBackpackTankComponent>(uid))
                continue;

            if (!TryComp<RMCFlamerTankComponent>(uid, out var tankComponent))
                continue;

            if (!_solution.TryGetSolution(uid, tankComponent.SolutionId, out solutionEntity, out _))
                continue;

            tankEntity = new Entity<RMCFlamerTankComponent>(uid, tankComponent);
            solutionEntity = new Entity<SolutionComponent>(uid, solutionEntity);
            return true;
        }

        return false;
    }
}
