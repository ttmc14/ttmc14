using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.Events;
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

    protected virtual void OnViewerMove(Entity<CEZLevelViewerComponent> entity, ref MoveEvent args)
    {
        if (!entity.Comp.LookUp)
            return;

        if (!HasOpaqueAbove(entity))
            return;

        entity.Comp.LookUp = false;
        DirtyField(entity, entity.Comp, nameof(CEZLevelViewerComponent.LookUp));
    }

    private void OnToggleLookUp(Entity<CEZLevelViewerComponent> entity, ref CEToggleZLevelLookUpAction args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (HasOpaqueAbove(entity))
        {
            _popup.PopupClient(Loc.GetString("ce-zlevel-look-up-fail"), entity, entity);
            return;
        }

        entity.Comp.LookUp = !entity.Comp.LookUp;
        DirtyField(entity, entity.Comp, nameof(CEZLevelViewerComponent.LookUp));
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
