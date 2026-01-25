using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Tag;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce.Trail;

public sealed class MCXenoPounceTrailSystem : EntitySystem
{
    private static readonly ProtoId<TagPrototype> AcidSprayTag = "MCAcidSpray";

    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly TagSystem _tag = null!;

    [Dependency] private readonly SharedXenoHiveSystem _rmcHive = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPouncingComponent, MoveEvent>(OnMove);
    }

    private void OnMove(Entity<MCXenoPouncingComponent> entity, ref MoveEvent args)
    {
        if (!TryComp<MCXenoPounceTrailComponent>(entity, out var component))
            return;

        var coordinates = Transform(entity).Coordinates;
        if (_transform.GetGrid(coordinates) is not { } gridId || !TryComp<MapGridComponent>(gridId, out var grid))
            return;

        var tile = _map.TileIndicesFor(gridId, grid, coordinates);
        if (component.LastTurf is not null && component.LastTurf == tile)
            return;

        component.LastTurf = tile;
        Dirty(entity, component);

        var anchored = _map.GetAnchoredEntitiesEnumerator(gridId, grid, tile);
        while (anchored.MoveNext(out var uid))
        {
            if (_tag.HasTag(uid.Value, AcidSprayTag))
                return;
        }

        if (_net.IsClient)
            return;

        var spawn = SpawnAtPosition(component.TrailId, coordinates);
        _rmcHive.SetSameHive(entity.Owner, spawn);
    }
}
