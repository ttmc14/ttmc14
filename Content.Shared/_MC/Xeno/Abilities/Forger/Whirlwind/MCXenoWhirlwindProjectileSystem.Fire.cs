using Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind.Components;
using Content.Shared._RMC14.Atmos;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind;

public sealed partial class MCXenoWhirlwindProjectileSystem
{
    private void SpreadFire(Entity<MCXenoWhirlwindProjectileComponent> entity, TileRef center)
    {
        var grid = Comp<MapGridComponent>(center.GridUid);

        IgniteTile(entity, center, grid);
        IgniteNeighbor(entity, center, grid);
    }

    private bool CanBurn(TileRef tile) =>
        !tile.Tile.IsEmpty && !_turf.IsTileBlocked(tile, Obstacles);

    private void IgniteTile(Entity<MCXenoWhirlwindProjectileComponent> entity, TileRef tile, MapGridComponent grid)
    {
        if (!CanBurn(tile))
            return;

        if (GetFire(entity, tile, grid) is { } fire)
        {
            RefreshFire(entity, fire);
            return;
        }

        SpawnFire(entity, tile);
    }

    private void IgniteNeighbor(Entity<MCXenoWhirlwindProjectileComponent> entity, TileRef center, MapGridComponent grid)
    {
        var directions = new List<Direction>(DirectionExtensions.AllDirections);
        _random.Shuffle(directions);

        Entity<TileFireComponent>? existingFire = null;

        foreach (var direction in directions)
        {
            var indices = center.GridIndices + direction.ToIntVec();

            if (!_map.TryGetTileRef(center.GridUid, grid, indices, out var tile) || !CanBurn(tile))
                continue;

            if (GetFire(entity, tile, grid) is { } fire)
            {
                existingFire ??= fire;
                continue;
            }

            SpawnFire(entity, tile);
            return;
        }

        if (existingFire is { } neighbor)
            RefreshFire(entity, neighbor);
    }

    private Entity<TileFireComponent>? GetFire(Entity<MCXenoWhirlwindProjectileComponent> entity, TileRef tile, MapGridComponent grid)
    {
        var anchored = _map.GetAnchoredEntitiesEnumerator(tile.GridUid, grid, tile.GridIndices);

        while (anchored.MoveNext(out var uid))
        {
            if (!TryComp<TileFireComponent>(uid, out var fire) || fire.Id != entity.Comp.SpawnId)
                continue;

            if (!EntityManager.IsQueuedForDeletion(uid.Value))
                return (uid.Value, fire);
        }

        return null;
    }

    private void SpawnFire(Entity<MCXenoWhirlwindProjectileComponent> entity, TileRef tile)
    {
        var fireUid = Spawn(entity.Comp.SpawnId, _map.ToCenterCoordinates(tile));
        MCXenoHive.SetSameHive(entity.Owner, fireUid);
    }

    private void RefreshFire(EntityUid whirlwind, Entity<TileFireComponent> fire)
    {
        _rmcFlammable.RefreshTileFire(fire);
        MCXenoHive.SetSameHive(whirlwind, fire.Owner);
    }
}
