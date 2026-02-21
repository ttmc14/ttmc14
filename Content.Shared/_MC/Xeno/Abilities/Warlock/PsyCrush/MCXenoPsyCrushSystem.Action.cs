using System.Linq;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Physics;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.PsyCrush;

public sealed partial class MCXenoPsyCrushSystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;

    private void OnAction(Entity<MCXenoPsyCrushComponent> entity, ref MCXenoPsyCrushActionEvent args)
    {
        if (args.Handled || RemCompDeferred<MCXenoPsyCrushActiveComponent>(entity))
            return;

        if (!CanUseAction(entity, args.Action))
            return;

        var centered = GetCenteredCoordinates(args.Target);
        if (!InRange(entity, centered))
            return;

        var doAfter = new DoAfterArgs(
            EntityManager,
            entity.Owner,
            entity.Comp.Delay,
            new MCXenoPsyCrushDoAfterEvent(GetNetCoordinates(centered)),
            entity.Owner)
        {
            BreakOnMove = false,
            AttemptFrequency = AttemptFrequency.EveryTick,
#pragma warning disable CS0618 // Type or member is obsolete
            ExtraCheck = () => InRange(entity, centered),
#pragma warning restore CS0618 // Type or member is obsolete
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfter(Entity<MCXenoPsyCrushComponent> entity, ref MCXenoPsyCrushDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        var centered = GetCoordinates(args.Coordinates);

        var mapCoords = _transform.ToMapCoordinates(centered);
        if (!_mapManager.TryFindGridAt(mapCoords, out var gridUid, out var grid))
            return;

        var activeComponent = EnsureComp<MCXenoPsyCrushActiveComponent>(entity.Owner);

        activeComponent.TargetCoords = centered;
        activeComponent.GridUid = gridUid;
        activeComponent.CenterTile = _map.LocalToTile(gridUid, grid, centered);
        activeComponent.NextExpansion = TimeSpan.Zero;

        var orbUid = ServerSpawn(entity.Comp.OrbEffectId, centered);
        if (orbUid.Valid)
            activeComponent.OrbUid = orbUid;

        if (entity.Comp.EffectSoundAction is not null)
            _audio.PlayPredicted(entity.Comp.EffectSoundAction, entity, entity);

        ActionSetState<MCXenoPsyCrushActionEvent>(entity, "crush_activate");
    }

    private void StopAction(EntityUid uid)
    {
        RemCompDeferred<MCXenoPsyCrushActiveComponent>(uid);
    }

    private bool InRange(
        Entity<MCXenoPsyCrushComponent> entity,
        EntityCoordinates target,
        bool sightNeeded = true)
    {
        var ownerCoordinates = Transform(entity).Coordinates;
        var direction = target.Position - ownerCoordinates.Position;
        var distance = direction.Length();

        if (distance > entity.Comp.Range)
        {
            _popup.PopupClient("Too far!", entity, entity);
            return false;
        }

        if (!sightNeeded)
            return true;

        var ray = new CollisionRay(
            ownerCoordinates.Position,
            direction.Normalized(),
            (int) CollisionGroup.Opaque
        );

        var results = _physics.IntersectRayWithPredicate(_transform.GetMapId(entity.Owner),
            ray,
            distance,
            ent => ent == entity.Owner || HasComp<MobStateComponent>(ent),
            false);

        if (!results.Any())
            return true;

        _popup.PopupClient("Out of sight!!", entity, entity);
        return false;
    }
}
