using System.Numerics;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Coordinates;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Damage;
using Content.Shared.Actions;
using Content.Shared.Maps;
using Content.Shared.Atmos.Components;
using Content.Shared.Interaction;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using static Content.Shared.Physics.CollisionGroup;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Inferno;

public sealed class MCXenoInfernoSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = null!;
    [Dependency] private readonly SharedMapSystem _mapSystem = null!;
    [Dependency] private readonly TurfSystem _turf = null!;
    [Dependency] private readonly XenoSystem _xeno = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedInteractionSystem _interaction = null!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = null!;

    private readonly HashSet<Entity<MobStateComponent>> _receivers = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoInfernoComponent, MCXenoInfernoActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoInfernoComponent, MCXenoInfernoDoAfterEvent>(OnXenoInfernoDoAfter);
    }

    private void OnAction(Entity<MCXenoInfernoComponent> entity, ref MCXenoInfernoActionEvent args)
    {
        if (args.Handled)
            return;

        if (!RMCActions.CanUseActionPopup(entity.Owner, entity))
            return;

        var ev = new MCXenoInfernoDoAfterEvent(GetNetEntity(args.Action));
        var doAfter = new DoAfterArgs(EntityManager, entity, entity.Comp.InfernoDelay, ev, entity, entity)
        {
            BreakOnMove = true,
            BreakOnDamage = false,
            ForceVisible = true,
            CancelDuplicate = true,
            DuplicateCondition = DuplicateConditions.SameEvent,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnXenoInfernoDoAfter(Entity<MCXenoInfernoComponent> entity, ref MCXenoInfernoDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        var action = GetEntity(args.Action);
        if (!RMCActions.TryUseAction(entity, action, entity))
            return;

        args.Handled = true;

        if (!_net.IsServer)
            return;

        _audio.PlayPvs(entity.Comp.Sound, entity);
        SpawnAttachedTo(entity.Comp.Effect, entity.Owner.ToCoordinates());

        var transform = Transform(entity);

        _receivers.Clear();
        _entityLookup.GetEntitiesInRange(transform.Coordinates, entity.Comp.Range, _receivers);

        var center = transform.Coordinates;
        for (var x = -entity.Comp.PositionInfernoX; x <= entity.Comp.PositionInfernoX; x++)
        {
            for (var y = -entity.Comp.PositionInfernoY; y <= entity.Comp.PositionInfernoY; y++)
            {
                var offsetPosition = center.Offset(new Vector2(x, y));

                if (!CanPlaceFire(offsetPosition))
                    continue;

                if (!_interaction.InRangeUnobstructed(entity.Owner, offsetPosition, entity.Comp.Range))
                    continue;

                var fire = Spawn(entity.Comp.Spawn, offsetPosition);
                _xenoHive.SetSameHive(entity.Owner, fire);
            }
        }

        foreach (var receiver in _receivers)
        {
            if (_mobState.IsDead(receiver))
                continue;

            if (!_xeno.CanAbilityAttackTarget(entity, receiver))
                continue;

            _damageable.TryChangeDamage(
                receiver,
                _xeno.TryApplyXenoSlashDamageMultiplier(receiver, entity.Comp.Damage),
                origin: entity,
                tool: entity);

            if (!TryComp<FlammableComponent>(receiver, out var fireStacksComp))
                continue;

            fireStacksComp.FireStacks += 2;
            Dirty(receiver, fireStacksComp);
        }

        ActionStartUseDelay<MCXenoInfernoActionEvent>(entity);
    }

    private bool CanPlaceFire(EntityCoordinates coords)
    {
        if (_transform.GetGrid(coords) is not { } gridId ||
            !TryComp<MapGridComponent>(gridId, out var grid))
            return false;

        var tile = _mapSystem.TileIndicesFor(gridId, grid, coords);
        return !_turf.IsTileBlocked(gridId, tile, Impassable | MidImpassable | HighImpassable, grid);
    }
}
