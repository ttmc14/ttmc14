/*
 * Copyright (c) 2026 TornadgoTechnology
 * Copyright (c) 2026 CrystallEdge (https://github.com/crystallpunk-14/crystall-edge)
 *
 * SPDX-License-Identifier: PolyForm-Noncommercial-1.0.0 AND MIT
 */

using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared.Actions;
using Content.Shared.Maps;
using Robust.Shared.Map;

namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem
{
    [Dependency] protected readonly ITileDefinitionManager TilDefMan = null!;

    private void InitializeView()
    {
        SubscribeLocalEvent<CEZLevelViewerComponent, MoveEvent>(OnViewerMove);
        SubscribeLocalEvent<CEZLevelViewerComponent, CEToggleZLevelLookUpAction>(OnToggleLookUp);
    }

    protected virtual void OnViewerMove(Entity<CEZLevelViewerComponent> ent, ref MoveEvent args)
    {
        if (!ent.Comp.LookUp)
            return;

        if (!HasOpaqueAbove(ent))
            return;

        ent.Comp.LookUp = false;
        DirtyField(ent, ent.Comp, nameof(CEZLevelViewerComponent.LookUp));
    }

    private void OnToggleLookUp(Entity<CEZLevelViewerComponent> ent, ref CEToggleZLevelLookUpAction args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (HasOpaqueAbove(ent))
        {
            _popup.PopupClient(Loc.GetString("ce-zlevel-look-up-fail"), ent, ent);
            return;
        }

        ent.Comp.LookUp = !ent.Comp.LookUp;
        DirtyField(ent, ent.Comp, nameof(CEZLevelViewerComponent.LookUp));
    }

    public bool LoockedUp(EntityUid uid)
    {
        return TryComp<CEZLevelViewerComponent>(uid, out var comp) && comp.LookUp;
    }

    public bool HasOpaqueAbove(EntityUid ent, Entity<CEZLevelMapComponent?>? currentMapUid = null)
    {
        currentMapUid ??= Transform(ent).MapUid;

        if (currentMapUid is null)
            return false;

        if (!TryMapUp(currentMapUid.Value, out var mapAboveUid))
            return false;

        if (!_gridQuery.TryComp(mapAboveUid, out var mapAboveGrid))
            return false;

        if (!_map.TryGetTileRef(mapAboveUid, mapAboveGrid, _transform.GetWorldPosition(ent), out var tileRef))
            return false;

        var tileDef = (ContentTileDefinition)TilDefMan[tileRef.Tile.TypeId];
        return !tileDef.Transparent;
    }
}

public sealed partial class CEToggleZLevelLookUpAction : InstantActionEvent;
