using System.Diagnostics.CodeAnalysis;
using Content.Shared._MC.Popup;
using Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector.Events;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.CreateBomb;

public sealed class MCXenoGlobSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoGlobComponent, MCXenoReagentSelectorSetEvent>(OnRelaySet);
        SubscribeLocalEvent<MCXenoGlobComponent, MCXenoCreateGlobActionEvent>(OnAction);
    }

    private void OnRelaySet(Entity<MCXenoGlobComponent> entity, ref MCXenoReagentSelectorSetEvent args)
    {
        // Just grabbing event from reagent selectors
        // I don't have desire to churn yet another UI
        if (!entity.Comp.Entries.TryGetValue(args.Key, out var entry))
        {
            Log.Warning($"Not found relayed entry: {entry}");
            return;
        }

        entity.Comp.SelectedEntry = entry;
        DirtyField(entity, entity.Comp, nameof(MCXenoGlobComponent.SelectedEntry));
    }

    private void OnAction(Entity<MCXenoGlobComponent> entity, ref MCXenoCreateGlobActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryUseAction(entity, args.Action))
            return;

        if (!TryAddGlobCount((entity, entity), args.Amount, args.Action, true))
            return;

        args.Handled = true;
    }

    public bool TryAddGlobCount(Entity<MCXenoGlobComponent?> entity, int amount = 1, EntityUid? targetActionUid = null, bool popup = false)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return false;

        if (amount <= 0)
        {
            Log.Fatal("Amount <= 0");
            return false;
        }

        var count = entity.Comp.Count;
        var countMax = entity.Comp.CountMax;

        var newCount = count + amount;

        if (newCount <= countMax)
        {
            SetGlobCount(entity, newCount, targetActionUid, popup);
            return true;
        }

        if (count < countMax)
        {
            SetGlobCount(entity, countMax, targetActionUid, popup);
            return true;
        }

        if (popup)
            _popup.PopupLocEntServer(entity, "mc-xeno-ability-glob-max", PopupType.MediumCaution);

        return false;
    }

    public bool TryRemoveGlobCount(Entity<MCXenoGlobComponent?> entity, int amount = 1, EntityUid? targetActionUid = null, bool popup = false)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return false;

        if (amount <= 0)
        {
            Log.Fatal("Amount <= 0");
            return false;
        }

        var count = entity.Comp.Count;
        var newCount = count - amount;

        if (newCount >= 0)
        {
            SetGlobCount(entity, newCount, targetActionUid);
            return true;
        }

        if (popup)
            _popup.PopupLocEntServer(entity, "mc-xeno-ability-glob-min", PopupType.MediumCaution);

        return false;
    }

    public void SetGlobCount(Entity<MCXenoGlobComponent?> entity, int count, EntityUid? targetActionUid = null, bool popup = false)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        if (entity.Comp.Count == count)
            return;

        entity.Comp.Count = count;
        DirtyField(entity, nameof(MCXenoGlobComponent.Count));
        RefreshActionIcon(entity, targetActionUid);

        if (popup)
            _popup.PopupEntServer(Loc.GetString("mc-xeno-ability-glob", ("count", count), ("max", entity.Comp.CountMax)), entity);
    }

    public bool HasGlobCount(Entity<MCXenoGlobComponent?> entity, int amount = 1)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return false;

        return entity.Comp.Count >= amount;
    }

    public int GetGlobCount(Entity<MCXenoGlobComponent?> entity)
    {
        return Resolve(entity, ref entity.Comp, logMissing: false) ? entity.Comp.Count : 0;
    }

    public bool TryGetShroudId(Entity<MCXenoGlobComponent?> entity, out EntProtoId shroudId)
    {
        shroudId = default;
        if (!TryGetEntry(entity, out var entry))
            return false;

        shroudId = entry.ShroudGlobId;
        return true;
    }

    public bool TryGetGlobId(Entity<MCXenoGlobComponent?> entity, out EntProtoId globId)
    {
        globId = default;
        if (!TryGetEntry(entity, out var entry))
            return false;

        globId = entry.GlobId;
        return true;
    }

    public bool TryGetEntry(Entity<MCXenoGlobComponent?> entity, [NotNullWhen(true)] out MCXenoGlobComponent.Entry? entry)
    {
        entry = null;
        if (!Resolve(entity, ref entity.Comp, logMissing: false) || entity.Comp.SelectedEntry is null)
            return false;

        entry = entity.Comp.SelectedEntry;
        return true;
    }

    private void RefreshActionIcon(Entity<MCXenoGlobComponent?> entity, EntityUid? targetActionUid = null)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        foreach (var actionEntity in RMCActions.GetActionsWithEvent<MCXenoCreateGlobActionEvent>(entity))
        {
            if (targetActionUid is not null && targetActionUid != actionEntity.Owner)
                continue;

            if (Actions.GetEvent(actionEntity) is not MCXenoCreateGlobActionEvent ev)
                continue;

            if (actionEntity.Comp.Icon is not SpriteSpecifier.Rsi icon)
                continue;

            Actions.SetIcon((actionEntity, actionEntity), new SpriteSpecifier.Rsi(icon.RsiPath, $"{ev.StatePrefix}{entity.Comp.Count}"));
        }
    }
}
