using System.Diagnostics.CodeAnalysis;
using Content.Shared._RMC14.Areas;
using Content.Shared._RMC14.Map;
using Content.Shared._RMC14.Sentry;
using Content.Shared._RMC14.Xenonids.Announce;
using Content.Shared._RMC14.Xenonids.Construction;
using Content.Shared._RMC14.Xenonids.Construction.Nest;
using Content.Shared._RMC14.Xenonids.Construction.Tunnel;
using Content.Shared._RMC14.Xenonids.Egg;
using Content.Shared._RMC14.Xenonids.Eye;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared._RMC14.Xenonids.Weeds;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Buckle.Components;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using static Content.Shared.Physics.CollisionGroup;

namespace Content.Shared._MC.Xeno.Construction;

public sealed class MCXenoConstructionSystem : EntitySystem
{
    private static readonly ProtoId<TagPrototype> AirlockTag = "Airlock";
    private static readonly ProtoId<TagPrototype> StructureTag = "Structure";

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly AreaSystem _area = default!;
    [Dependency] private readonly SharedXenoAnnounceSystem _announce = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogs = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedXenoHiveSystem _hive = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly QueenEyeSystem _queenEye = default!;
    [Dependency] private readonly RMCMapSystem _rmcMap = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly XenoNestSystem _xenoNest = default!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly SharedXenoWeedsSystem _xenoWeeds = default!;
    [Dependency] private readonly ITileDefinitionManager _tile = default!;

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
