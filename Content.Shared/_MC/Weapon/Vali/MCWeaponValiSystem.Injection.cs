using System.Diagnostics.CodeAnalysis;
using Content.Shared._MC.Weapon.Vali.Components;
using Content.Shared._MC.Weapon.Vali.Events.DoAfter;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;

namespace Content.Shared._MC.Weapon.Vali;

public sealed partial class MCWeaponValiSystem
{
    private void InitializeInjection()
    {
        SubscribeLocalEvent<MCWeaponValiComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<MCWeaponValiComponent, MCWeaponValiFillDoAfterEvent>(OnFillDoAfter);
    }

    private void OnInteractUsing(Entity<MCWeaponValiComponent> entity, ref InteractUsingEvent args)
    {
        if (!TryGetSolutionId(entity, args.Used, out var solutionId, out _))
            return;

        if (!CanFill(entity, args.Used, solutionId))
            return;

        args.Handled = true;

        var used = args.Used;
        var ev = new MCWeaponValiFillDoAfterEvent(args.Used, EntityManager);
        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, entity.Comp.ReagentFillDelay, ev, entity, entity)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            BreakOnDropItem = true,
            BreakOnHandChange = true,
            CancelDuplicate = false,
            AttemptFrequency = AttemptFrequency.EveryTick,
#pragma warning disable CS0618 // Type or member is obsolete
            ExtraCheck = () => Transform(entity).ParentUid == Transform(used).ParentUid,
#pragma warning restore CS0618 // Type or member is obsolete
        };

        _doAfter.TryStartDoAfter(doAfterArgs, out entity.Comp.ReagentFillDoAfterId);
    }

    private void OnFillDoAfter(Entity<MCWeaponValiComponent> entity, ref MCWeaponValiFillDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!TryFill(entity, GetEntity(args.UsedUid)))
            return;

        args.Handled = true;
    }

    private bool TryFill(Entity<MCWeaponValiComponent> entity, EntityUid usedUid)
    {
        if (!TryGetSolutionId(entity, usedUid, out var solutionId, out var shouldDelete))
            return false;

        if (!_solutionContainer.TryGetSolution(usedUid, solutionId, out var solutionEntity, out var solution))
            return false;

        var removedReagents = new List<ReagentQuantity>();
        var transfer = false;

        foreach (var reagent in solution.Contents)
        {
            var reagentId = reagent.Reagent.Prototype;
            if (!entity.Comp.ReagentData.ContainsKey(reagentId))
                continue;

            var currentAmount = entity.Comp.Reagents.GetValueOrDefault(reagentId, FixedPoint2.Zero);
            var spaceLeft = entity.Comp.ReagentCapacity - currentAmount;

            if (spaceLeft <= 0)
                continue;

            var quantityToTransfer = FixedPoint2.Min(reagent.Quantity, spaceLeft);
            entity.Comp.Reagents[reagentId] = currentAmount + quantityToTransfer;
            removedReagents.Add(new ReagentQuantity(reagentId, quantityToTransfer));
            transfer = true;
        }

        foreach (var reagent in removedReagents)
        {
            _solutionContainer.RemoveReagent(solutionEntity.Value, reagent);
        }

        Dirty(entity);

        if (!transfer)
            return false;

        if (!shouldDelete)
            return true;

        PredictedDel(usedUid);
        return true;
    }

    private bool CanFill(Entity<MCWeaponValiComponent> entity, EntityUid usedUid, string solutionId)
    {
        if (!_solutionContainer.TryGetSolution(usedUid, solutionId, out _, out var solution))
            return false;

        foreach (var reagent in solution.Contents)
        {
            var reagentId = reagent.Reagent.Prototype;

            if (!entity.Comp.ReagentData.ContainsKey(reagentId))
                continue;

            var currentAmount = entity.Comp.Reagents.GetValueOrDefault(reagentId, FixedPoint2.Zero);
            if (currentAmount < entity.Comp.ReagentCapacity)
                return true;
        }

        return false;
    }

    private bool TryGetSolutionId(Entity<MCWeaponValiComponent> _, EntityUid usedUid, [NotNullWhen(true)] out string? solution, out bool shouldDelete)
    {
        solution = null;
        shouldDelete = false;

        if (TryComp<SolutionSpikerComponent>(usedUid, out var solutionSpikerComponent))
        {
            solution = solutionSpikerComponent.SourceSolution;
            shouldDelete = true;
            return true;
        }

        if (TryComp<HyposprayComponent>(usedUid, out var hyposprayComponent) && !hyposprayComponent.OnlyAffectsMobs)
        {
            solution = hyposprayComponent.SolutionName;
            return true;
        }

        if (TryComp<DrawableSolutionComponent>(usedUid, out var drawableSolutionComponent))
        {
            solution = drawableSolutionComponent.Solution;
            return true;
        }

        return false;
    }
}
