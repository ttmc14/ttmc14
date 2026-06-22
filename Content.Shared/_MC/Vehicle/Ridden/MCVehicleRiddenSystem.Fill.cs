using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._MC.Vehicle.Ridden.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Whitelist;

namespace Content.Shared._MC.Vehicle.Ridden;

public sealed partial class MCVehicleRiddenSystem
{
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = null!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = null!;

    private void OnInteractUsing(Entity<MCVehicleRiddenComponent> entity, ref InteractUsingEvent args)
    {
        if (!_entityWhitelist.IsWhitelistPassOrNull(entity.Comp.RefillWhitelist, args.Used))
            return;

        if (!CanFill(entity, args.Used))
            return;

        if (!TryFill(entity, args.Used))
            return;

        args.Handled = true;
    }

    private bool TryFill(Entity<MCVehicleRiddenComponent> entity, EntityUid usedUid)
    {
        if (!TryGetSolutionId(usedUid, out var solutionId))
            return false;

        if (!_solutionContainer.TryGetSolution(usedUid, solutionId, out var solutionEntity, out var solution))
            return false;

        var fuelSpace = entity.Comp.FuelMax - entity.Comp.Fuel;

        if (fuelSpace <= 0f)
            return false;

        var maxTransfer = fuelSpace;

        if (TryComp<SolutionTransferComponent>(usedUid, out var transfer))
            maxTransfer = float.Min(maxTransfer, transfer.TransferAmount.Float());

        var removedReagents = new List<ReagentQuantity>();
        var transferred = 0f;

        foreach (var reagent in solution.Contents)
        {
            var reagentId = reagent.Reagent.Prototype;

            if (!entity.Comp.AllowedReagents.TryGetValue(reagentId, out var multiplier))
                continue;

            var possibleFuel = reagent.Quantity.Float() * multiplier;
            if (possibleFuel <= 0f)
                continue;

            var fuelToAdd = float.Min(possibleFuel, maxTransfer - transferred);
            if (fuelToAdd <= 0f)
                continue;

            var reagentToRemove = fuelToAdd / multiplier;

            removedReagents.Add(new ReagentQuantity(reagentId, FixedPoint2.New(reagentToRemove)));
            transferred += fuelToAdd;

            if (transferred >= maxTransfer)
                break;
        }

        if (transferred <= 0f)
            return false;

        foreach (var reagent in removedReagents)
        {
            _solutionContainer.RemoveReagent(solutionEntity.Value, reagent);
        }


        entity.Comp.Fuel += transferred;
        Dirty(entity);

        _actionBlocker.UpdateCanMove(entity);

        return true;
    }

    private bool CanFill(Entity<MCVehicleRiddenComponent> entity, EntityUid usedUid)
    {
        if (entity.Comp.Fuel >= entity.Comp.FuelMax)
            return false;

        if (!TryGetSolutionId(usedUid, out var solutionId))
            return false;

        return _solutionContainer.TryGetSolution(usedUid, solutionId, out _, out var solution)
            && solution.Contents.Any(reagent => entity.Comp.AllowedReagents.ContainsKey(reagent.Reagent.Prototype)
            && reagent.Quantity.Float() > 0);
    }

    private bool TryGetSolutionId(EntityUid usedUid, [NotNullWhen(true)] out string? solution)
    {
        solution = null;

        if (!TryComp<DrainableSolutionComponent>(usedUid, out var drawableSolutionComponent))
            return false;

        solution = drawableSolutionComponent.Solution;
        return true;
    }
}
