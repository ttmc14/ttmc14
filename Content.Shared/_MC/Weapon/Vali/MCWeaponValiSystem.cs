using System.Diagnostics.CodeAnalysis;
using System.Text;
using Content.Shared._MC.Utilities;
using Content.Shared._MC.Weapon.Vali.Ui;
using Content.Shared._RMC14.Chemistry.Reagent;
using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Weapon.Vali;

public sealed partial class MCWeaponValiSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly SharedActionsSystem _actions = null!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = null!;
    [Dependency] private readonly RMCReagentSystem _rmcReagentSystem = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCWeaponValiComponent, MeleeAttackEvent>(OnMeleeAttack);
        SubscribeLocalEvent<MCWeaponValiComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<MCWeaponValiComponent, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<MCWeaponValiComponent, MCWeaponValiSelectReagentAction>(OnSelectReagentAction);
        SubscribeLocalEvent<MCWeaponValiComponent, MCWeaponValiSelectReagentMessage>(OnSelectReagentMessage);
        SubscribeLocalEvent<MCWeaponValiComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<MCWeaponValiComponent, ExaminedEvent>(OnExamine);
    }

    private void OnGetItemActions(Entity<MCWeaponValiComponent> entity, ref GetItemActionsEvent args)
    {
        args.AddAction(ref entity.Comp.ActionSelectReagent, entity.Comp.ActionSelectReagentId);
        Dirty(entity);
    }

    private void OnSelectReagentAction(Entity<MCWeaponValiComponent> entity, ref MCWeaponValiSelectReagentAction args)
    {
        _userInterface.TryOpenUi(entity.Owner, MCWeaponValiSelectReagentUi.Key, args.Performer);
        args.Handled = true;
    }

    private void OnSelectReagentMessage(Entity<MCWeaponValiComponent> entity, ref MCWeaponValiSelectReagentMessage args)
    {
        _userInterface.CloseUi(entity.Owner, MCWeaponValiSelectReagentUi.Key);
        SelectReagent(entity, args.ReagentId);
    }

    private void OnInteractUsing(Entity<MCWeaponValiComponent> entity, ref InteractUsingEvent args)
    {
        if (!TryFill(entity, args.Used))
            return;

        args.Handled = true;
    }

    private void OnExamine(Entity<MCWeaponValiComponent> entity, ref ExaminedEvent args)
    {
        var stringBuilder = new StringBuilder();

        if (entity.Comp.Reagents.Count > 0)
        {
            stringBuilder.AppendLine(Loc.GetString("mc-weapon-vali-hold"));
            foreach (var (reagentId, value) in entity.Comp.Reagents)
            {
                if (!_rmcReagentSystem.TryIndex(reagentId, out var reagent))
                    continue;

                stringBuilder.AppendReagent(reagent);
                stringBuilder.Append(" - ");
                stringBuilder.Append(value.ToString());
                stringBuilder.AppendLine("u");
            }
        }

        stringBuilder.AppendLine(Loc.GetString("mc-weapon-vali-compatible"));
        foreach (var reagentId in entity.Comp.AllowedReagents)
        {
            if (!_rmcReagentSystem.TryIndex(reagentId, out var reagent))
                continue;

            stringBuilder.AppendLineReagent(reagent);
        }

        args.PushMarkup(stringBuilder.ToString(), priority: 10);
    }

    private bool TryFill(Entity<MCWeaponValiComponent> entity, EntityUid usedUid)
    {
        if (!TryGetSolutionId(entity, usedUid, out var solutionId, out var shouldDelete))
            return false;

        if (!_solutionContainer.TryGetSolution(usedUid, solutionId, out _, out var solution))
            return false;

        var removedReagents = new List<(string, FixedPoint2)>();
        var transfer = false;
        foreach (var reagent in solution.Contents)
        {
            var reagentId = reagent.Reagent.Prototype;
            if (!entity.Comp.AllowedReagents.Contains(reagentId))
                continue;

            if (!entity.Comp.Reagents.TryGetValue(reagentId, out var value))
            {
                entity.Comp.Reagents[reagentId] = FixedPoint2.Zero;
                value = FixedPoint2.Zero;
            }

            if (value >= entity.Comp.ReagentCapacity)
                continue;

            var quantity = reagent.Quantity > entity.Comp.ReagentCapacity + value ? reagent.Quantity - entity.Comp.ReagentCapacity : reagent.Quantity;
            entity.Comp.Reagents[reagentId] = FixedPoint2.Clamp(value + quantity, 0, entity.Comp.ReagentCapacity);
            removedReagents.Add((reagentId, quantity));

            transfer = true;
        }

        foreach (var (reagentId, quantity) in removedReagents)
        {
            solution.RemoveReagent(reagentId, quantity);
        }

        Dirty(entity);

        if (!transfer)
            return false;

        if (!shouldDelete)
            return true;

        PredictedDel(usedUid);
        return true;
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

    private void SelectReagent(Entity<MCWeaponValiComponent> entity, ProtoId<ReagentPrototype>? reagentId)
    {
        if (reagentId is not null)
        {
            if (!entity.Comp.AllowedReagents.Contains(reagentId.Value))
                return;

            if (!entity.Comp.Reagents.TryGetValue(reagentId.Value, out var value) || value == FixedPoint2.Zero)
                return;
        }

        entity.Comp.SelectedReagent = reagentId;

        if (entity.Comp.ActionSelectReagent is {} actionUid)
            _actions.SetIcon(actionUid, reagentId.HasValue ? entity.Comp.ReagentIcons[reagentId.Value] : entity.Comp.ReagentEmptyIcon);

        _appearance.SetData(entity, MCWeaponValiVisuals.ReagentId, reagentId?.ToString() ?? string.Empty);

        var parentUid = Transform(entity).ParentUid;
        if (!HasComp<MapGridComponent>(parentUid))
            _appearance.SetData(parentUid, MCWeaponValiVisuals.ReagentId, reagentId?.ToString() ?? string.Empty);

        Dirty(entity);
    }
}
