using System.Numerics;
using Content.Shared._MC.Damage;
using Content.Shared._MC.Mob.Stamina;
using Content.Shared._MC.Xeno.Hive.Systems;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.PsyCrush;

public sealed partial class MCXenoPsyCrushSystem : MCXenoAbilitySystem
{
    private static readonly Vector2i[] Directions =
    {
        Vector2i.Up,
        Vector2i.Down,
        Vector2i.Left,
        Vector2i.Right
    };

    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly IMapManager _mapManager = null!;
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly TurfSystem _turf = null!;
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedPhysicsSystem _physics = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;

    [Dependency] private readonly MCXenoPlasmaSystem _plasma = null!;
    [Dependency] private readonly MCStaminaSystem _mcStamina = null!;
    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;
    [Dependency] private readonly MCDamageableSystem _mcDamageable = null!;

    private readonly HashSet<EntityUid> _affected = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPsyCrushComponent, MCXenoPsyCrushActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoPsyCrushComponent, MCXenoPsyCrushDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<MCXenoPsyCrushActiveComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MCXenoPsyCrushActiveComponent, MCXenoPsyCrushComponent>();
        while (query.MoveNext(out var uid, out var active, out var config))
        {
            Update((uid, active), config);
        }
    }

    private void Update(Entity<MCXenoPsyCrushActiveComponent> entity, MCXenoPsyCrushComponent config)
    {
        if (entity.Comp.NextExpansion > _timing.CurTime)
            return;

        entity.Comp.NextExpansion = _timing.CurTime + config.ExpansionDelay;

        if (!_plasma.TryRemovePlasma(entity, config.PlasmaCostPerStep))
        {
            StopAction(entity);
            return;
        }

        if (entity.Comp.CurrentRadius >= config.MaxExpansions)
        {
            StopAction(entity);
            return;
        }

        ExpandStep(entity, config);
    }

    private void ExpandStep(Entity<MCXenoPsyCrushActiveComponent> entity, MCXenoPsyCrushComponent config)
    {
        if (!TryComp<MapGridComponent>(entity.Comp.GridUid, out var grid))
            return;

        var nextFrontier = new Queue<Vector2i>();

        while (entity.Comp.Frontier.TryDequeue(out var tile))
        {
            foreach (var direction in Directions)
            {
                var next = tile + direction;

                if (!entity.Comp.Visited.Add(next))
                    continue;

                if (_turf.IsTileBlocked(entity.Comp.GridUid, next, CollisionGroup.Impassable | CollisionGroup.HighImpassable, grid))
                    continue;

                var coords = _map.GridTileToLocal(entity.Comp.GridUid, grid, next);
                var effect = ServerSpawn(config.WarningEffectId, coords);

                if (effect.Valid)
                    entity.Comp.SpawnedEffects.Add(effect);

                TryApplyTile(entity, config, tile, grid);

                nextFrontier.Enqueue(next);
            }
        }

        entity.Comp.Frontier = nextFrontier;
        entity.Comp.CurrentRadius++;

        _audio.PlayPredicted(config.EffectSoundExpand, entity.Comp.TargetCoords, entity);
    }

    private void TryApplyTile(
        Entity<MCXenoPsyCrushActiveComponent> entity,
        MCXenoPsyCrushComponent config,
        Vector2i tile,
        MapGridComponent grid)
    {
        if (!entity.Comp.AffectedTiles.Add(tile))
            return;

        var coords = _map.GridTileToLocal(entity.Comp.GridUid, grid, tile);
        var uid = ServerSpawn(config.WarningEffectId, coords);
        if (!uid.Valid)
            return;

        entity.Comp.SpawnedEffects.Add(uid);
    }

    private HashSet<EntityUid> GetPotentialVictims(
        Entity<MCXenoPsyCrushActiveComponent> entity,
        MCXenoPsyCrushComponent config)
    {
        if (!TryComp<MapGridComponent>(entity.Comp.GridUid, out var grid))
            return new HashSet<EntityUid>();

        var centerCoords = _map.GridTileToLocal(entity.Comp.GridUid, grid, entity.Comp.CenterTile);
        var radius = config.MaxExpansions + 0.5f;

        var box = Box2.CenteredAround(centerCoords.Position, new Vector2(radius, radius));
        return _lookup.GetEntitiesIntersecting(entity.Comp.GridUid, box);
    }

    private EntityCoordinates GetCenteredCoordinates(EntityCoordinates coords)
    {
        var map = _transform.ToMapCoordinates(coords);

        var tileX = (int) float.Floor(map.X);
        var tileY = (int) float.Floor(map.Y);

        var centered = new MapCoordinates(
            new Vector2(tileX + 0.5f, tileY + 0.5f),
            map.MapId);

        return _transform.ToCoordinates(centered);
    }
}
