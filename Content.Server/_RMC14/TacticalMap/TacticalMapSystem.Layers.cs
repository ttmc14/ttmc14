using Content.Server._CE.ZLevels.Core;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._RMC14.Areas;
using Content.Shared._RMC14.TacticalMap;
using Robust.Shared.Map.Components;

namespace Content.Server._RMC14.TacticalMap;

public sealed partial class TacticalMapSystem
{
    private readonly Dictionary<(EntityUid Owner, Enum Key, EntityUid Actor), EntityUid> _viewedLayers = [];

    private void OnZNetworkUpdated(CEZLevelNetworkUpdatedEvent args)
    {
        var network = args.Network;
        IReadOnlyDictionary<int, EntityUid?> levels = network.Comp.ZLevels;
        if (!levels.TryGetValue(0, out var root) || !HasComp<TacticalMapComponent>(root))
            return;

        EnsureTacticalMapLayers(network.Comp);

        var users = EntityQueryEnumerator<TacticalMapUserComponent>();
        while (users.MoveNext(out var uid, out var user))
        {
            if (user.Map == root && _ui.IsUiOpen(uid, TacticalMapUserUi.Key))
            {
                UpdateTacticalMapState((uid, user));
            }
        }

        var computers = EntityQueryEnumerator<TacticalMapComputerComponent>();
        while (computers.MoveNext(out var uid, out var computer))
        {
            if (computer.Map == root && _ui.IsUiOpen(uid, TacticalMapComputerUi.Key))
            {
                UpdateTacticalMapComputerState((uid, computer));
            }
        }
    }

    private void EnsureTacticalMapLayers(CEZLevelsNetworkComponent network)
    {
        foreach (var (map, depth) in network.ZLevelByEntity)
        {
            if (depth == 0 || !TryComp<MapGridComponent>(map, out var grid) || grid.ChunkCount == 0)
                continue;

            EnsureComp<AreaGridComponent>(map);
            EnsureComp<TacticalMapComponent>(map);
        }
    }

    private Dictionary<int, NetEntity> GetLayers(EntityUid? root)
    {
        var layers = new Dictionary<int, NetEntity>();
        if (root is not { } map || !HasComp<TacticalMapComponent>(map))
            return layers;

        layers.Add(0, GetNetEntity(map));
        if (!TryComp<CEZLevelMapComponent>(map, out var level) ||
            !TryComp<CEZLevelsNetworkComponent>(level.NetworkUid, out var network))
            return layers;

        foreach (var (depth, layer) in network.ZLevels)
        {
            if (depth != 0 && layer is { } uid && HasComp<TacticalMapComponent>(uid))
            {
                layers.Add(depth, GetNetEntity(uid));
            }
        }

        return layers;
    }

    private void OnUserSelectLayer(Entity<TacticalMapUserComponent> ent, ref TacticalMapSelectLayerMsg args)
        => SelectLayer(ent, ent.Comp.Map, args);

    private void OnComputerSelectLayer(Entity<TacticalMapComputerComponent> ent, ref TacticalMapSelectLayerMsg args)
        => SelectLayer(ent, ent.Comp.Map, args);

    private void OnComputerBUIClosed(Entity<TacticalMapComputerComponent> ent, ref BoundUIClosedEvent args)
        => _viewedLayers.Remove((ent.Owner, args.UiKey, args.Actor));

    private void SelectLayer(EntityUid owner, EntityUid? root, TacticalMapSelectLayerMsg args)
    {
        if (!GetLayers(root).ContainsValue(args.Map) ||
            !TryGetEntity(args.Map, out var uid) ||
            !TryComp<TacticalMapComponent>(uid, out var map))
            return;

        _viewedLayers[(owner, args.UiKey, args.Actor)] = uid.Value;
        SendLayer(owner, args.UiKey, args.Actor, (uid.Value, map));
    }

    private bool TryGetViewedLayer(EntityUid owner, BoundUserInterfaceMessage args, NetEntity expected,
        out Entity<TacticalMapComponent> layer)
    {
        layer = default;
        if (!_viewedLayers.TryGetValue((owner, args.UiKey, args.Actor), out var uid) ||
            !TryComp<TacticalMapComponent>(uid, out var map) || GetNetEntity(uid) != expected)
            return false;

        layer = (uid, map);
        return true;
    }

    private void SendLayer(EntityUid owner, Enum key, EntityUid actor, Entity<TacticalMapComponent> layer)
    {
        var (uid, map) = layer;
        var netMap = GetNetEntity(uid);
        if (key is TacticalMapComputerUi)
        {
            var data = new TacticalMapLayerDataMsg(netMap, GetComputerBlips(map), map.MarineLines, map.MarineLabels);
            _ui.ServerSendUiMessage(owner, key, data, actor);
            return;
        }

        if (!TryComp<TacticalMapUserComponent>(owner, out var user))
            return;

        var (blips, xenos, structures) = GetUserBlips((owner, user), map);
        foreach (var (id, blip) in xenos)
        {
            blips.TryAdd(id, blip);
        }

        foreach (var (id, blip) in structures)
        {
            blips.TryAdd(id, blip);
        }

        var lines = new List<TacticalMapLine>();
        var labels = new Dictionary<Vector2i, string>();
        if (user.Marines)
        {
            lines.AddRange(map.MarineLines);
            foreach (var (position, label) in map.MarineLabels)
            {
                labels[position] = label;
            }
        }

        if (user.Xenos)
        {
            lines.AddRange(map.XenoLines);
            foreach (var (position, label) in map.XenoLabels)
            {
                labels[position] = label;
            }
        }

        _ui.ServerSendUiMessage(owner, key, new TacticalMapLayerDataMsg(netMap, blips, lines, labels), actor);
    }

    private (Dictionary<int, TacticalMapBlip> Marines, Dictionary<int, TacticalMapBlip> Xenos,
        Dictionary<int, TacticalMapBlip> Structures) GetUserBlips(Entity<TacticalMapUserComponent> user, TacticalMapComponent map)
    {
        var marines = CopyVisibleBlips(user, user.Comp.Marines, map.MarineBlips, map.LastUpdateMarineBlips);
        var xenos = CopyVisibleBlips(user, user.Comp.Xenos, map.XenoBlips, map.LastUpdateXenoBlips);
        var structures = CopyVisibleBlips(user, user.Comp.Xenos, map.XenoStructureBlips, map.LastUpdateXenoStructureBlips);

        var visible = EntityQueryEnumerator<TacticalMapAlwaysVisibleComponent>();
        while (visible.MoveNext(out var uid, out var visibility))
        {
            if (FindBlipInMap(uid.Id, map) is not { } blip)
                continue;

            if (user.Comp.Marines && visibility.VisibleToMarines)
            {
                marines.TryAdd(uid.Id, blip);
            }

            if (user.Comp.Xenos && visibility.VisibleToXenos && !xenos.ContainsKey(uid.Id) && !structures.ContainsKey(uid.Id))
            {
                var target = visibility.VisibleAsXenoStructure ? structures : xenos;
                target.Add(uid.Id, blip);
            }
        }

        return (marines, xenos, structures);
    }

    private static Dictionary<int, TacticalMapBlip> CopyVisibleBlips(Entity<TacticalMapUserComponent> user, bool visible,
        Dictionary<int, TacticalMapBlip> live, Dictionary<int, TacticalMapBlip> snapshot)
    {
        if (!visible)
            return [];

        if (user.Comp.LiveUpdate)
            return new(live);

        var blips = new Dictionary<int, TacticalMapBlip>(snapshot);
        // Always show the viewer at their live position, including when they leave this layer.
        blips.Remove(user.Owner.Id);
        if (live.TryGetValue(user.Owner.Id, out var current))
        {
            blips.Add(user.Owner.Id, current);
        }

        return blips;
    }
}
