using Content.Shared._MC.Weapon.Vali.Components;
using Content.Shared._MC.Weapon.Vali.Events.DoAfter;
using Content.Shared._MC.Weapon.Vali.Visual;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.DoAfter;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Weapon.Vali;

public sealed partial class MCWeaponValiSystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;

    private void InitializeUsage()
    {
        SubscribeLocalEvent<MCWeaponValiComponent, MCWeaponValiSelectReagentDoAfterEvent>(OnSelectReagentDoAfter);
    }

    private void StartSelectReagentDoAfter(Entity<MCWeaponValiComponent> entity, EntityUid user, ProtoId<ReagentPrototype> reagent)
    {
        if (!entity.Comp.ReagentData.ContainsKey(reagent))
            return;

        if (!entity.Comp.Reagents.TryGetValue(reagent, out var value) || value < entity.Comp.ReagentUsage)
            return;

        var ev = new MCWeaponValiSelectReagentDoAfterEvent(reagent);
        var args = new DoAfterArgs(EntityManager, user, entity.Comp.ReagentSelectDelay, ev, entity, entity)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            BreakOnDropItem = true,
            CancelDuplicate = false,
        };

         _doAfter.TryStartDoAfter(args, out entity.Comp.ReagentSelectDoAfterId);
    }

    private void OnSelectReagentDoAfter(Entity<MCWeaponValiComponent> entity, ref MCWeaponValiSelectReagentDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        entity.Comp.ReagentSelectDoAfterId = null;

        SelectReagent(entity, args.ReagentId);
        args.Handled = true;
    }

    private void SelectReagent(Entity<MCWeaponValiComponent> entity, ProtoId<ReagentPrototype> reagent)
    {
        if (!entity.Comp.ReagentData.ContainsKey(reagent))
            return;

        if (!entity.Comp.Reagents.TryGetValue(reagent, out var value))
            return;

        if (value < entity.Comp.ReagentUsage)
            return;

        SetReagent(entity, reagent);
    }

    private void DeselectReagent(Entity<MCWeaponValiComponent> entity)
    {
        SetReagent(entity, null);
    }

    private static bool HasUsageAmount(Entity<MCWeaponValiComponent> entity, ProtoId<ReagentPrototype> reagentId)
    {
        if (!entity.Comp.Reagents.TryGetValue(reagentId, out var quantity))
            return false;

        return quantity >= entity.Comp.ReagentUsage;
    }

    private void SetReagent(Entity<MCWeaponValiComponent> entity, ProtoId<ReagentPrototype>? reagentId)
    {
        var state = reagentId?.Id ?? string.Empty;

        // Actions
        if (entity.Comp.ActionSelectReagent is { } actionUid)
        {
            var icon = entity.Comp.ReagentDefaultIcon;
            if (entity.Comp.ReagentData.TryGetValue(state, out var data))
                icon = data.Icon;

            _actions.SetIcon(actionUid, icon);
        }

        // Appearance self
        _appearance.SetData(entity, MCWeaponValiVisuals.ReagentId, state);

        // Appearance parent
        var parentUid = Transform(entity).ParentUid;
        if (!HasComp<MapGridComponent>(parentUid))
            _appearance.SetData(parentUid, MCWeaponValiVisuals.ReagentId, state);

        entity.Comp.Reagent = reagentId;
    }
}
