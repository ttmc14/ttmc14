using System.Diagnostics.CodeAnalysis;
using Content.Shared._RMC14.Sentry;
using Content.Shared._RMC14.Xenonids.Construction;
using Content.Shared._RMC14.Xenonids.Construction.Tunnel;
using Content.Shared._RMC14.Xenonids.Egg;
using Content.Shared._RMC14.Xenonids.Weeds;
using Content.Shared.Maps;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using static Content.Shared.Physics.CollisionGroup;

namespace Content.Shared._MC.Xeno.Construction;

public sealed class MCXenoConstructionSystem : EntitySystem
{
    private static readonly ProtoId<TagPrototype> AirlockTag = "Airlock";
    private static readonly ProtoId<TagPrototype> StructureTag = "Structure";

    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    private EntityQuery<BlockXenoConstructionComponent> _blockXenoConstructionQuery;
    private EntityQuery<SentryComponent> _sentryQuery;
    private EntityQuery<XenoTunnelComponent> _xenoTunnelQuery;

    public override void Initialize()
    {
        _blockXenoConstructionQuery = GetEntityQuery<BlockXenoConstructionComponent>();
        _sentryQuery = GetEntityQuery<SentryComponent>();
        _xenoTunnelQuery = GetEntityQuery<XenoTunnelComponent>();
    }

    public bool CanPlace(EntityUid user, EntityCoordinates coords, [NotNullWhen(false)] out string? popupType, bool needsWeeds = true)
    {
        popupType = null;
        if (_transform.GetGrid(coords) is not { } gridId || !TryComp<MapGridComponent>(gridId, out var grid))
        {
            popupType = "rmc-xeno-construction-no-map";
            return false;
        }

        var tile = _mapSystem.TileIndicesFor(gridId, grid, coords);
        var anchored = _mapSystem.GetAnchoredEntitiesEnumerator(gridId, grid, tile);
        var hasWeeds = false;

        while (anchored.MoveNext(out var uid))
        {
            if (HasComp<XenoEggComponent>(uid))
            {
                popupType = "rmc-xeno-construction-blocked";
                return false;
            }

            if (HasComp<XenoConstructComponent>(uid) ||
                _tags.HasAnyTag(uid.Value, StructureTag, AirlockTag) ||
                _xenoTunnelQuery.HasComp(uid) ||
                _sentryQuery.HasComp(uid) ||
                _blockXenoConstructionQuery.HasComp(uid))
            {
                popupType = "rmc-xeno-construction-blocked";
                return false;
            }

            if (HasComp<XenoWeedsComponent>(uid))
                hasWeeds = true;
        }

        if (_turf.IsTileBlocked(gridId, tile, Impassable | MidImpassable | HighImpassable, grid))
        {
            popupType = "rmc-xeno-construction-blocked";
            return false;
        }

        if (hasWeeds || !needsWeeds)
            return true;

        popupType = "rmc-xeno-construction-must-have-weeds";
        return false;
    }
}
