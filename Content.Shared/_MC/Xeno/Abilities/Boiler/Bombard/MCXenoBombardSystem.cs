using Content.Shared._MC.Popup;
using Content.Shared._MC.Xeno.Abilities.Boiler.Bombard.Components;
using Content.Shared._MC.Xeno.Abilities.Boiler.Bombard.Events.Actions;
using Content.Shared._MC.Xeno.Abilities.Boiler.Bombard.Events.DoAfter;
using Content.Shared._MC.Xeno.Abilities.Boiler.CreateBomb;
using Content.Shared._MC.Xeno.Spit;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Map;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.Bombard;

public sealed partial class MCXenoBombardSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly MCXenoGlobSystem _glob = null!;
    [Dependency] private readonly MCSharedXenoSpitSystem _spit = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeDigging();

        SubscribeLocalEvent<MCXenoBombardComponent, MCXenoBombardLaunchActionEvent>(OnLaunchAction);
        SubscribeLocalEvent<MCXenoBombardComponent, MCXenoBombardLaunchDoAfter>(OnLaunchDoAfter);
    }

    private void OnLaunchAction(Entity<MCXenoBombardComponent> entity, ref MCXenoBombardLaunchActionEvent args)
    {
        if (args.Handled)
            return;

        if (IsBusyDigging(entity) || TryDigging(entity, args.Action))
            return;

        if (!CanLaunch(entity, args.Target))
            return;

        if (!_spit.TryGetShotCoordinates(entity, args.Target, out _, out var target))
            return;

        var ev = new MCXenoBombardLaunchDoAfter(args.Action, args.Target, args.Entity, EntityManager)
        {
            TargetMap = target.MapId,
        };
        var doAfter = new DoAfterArgs(
            EntityManager,
            entity,
            entity.Comp.LaunchDuration,
            ev,
            entity
        );

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnLaunchDoAfter(Entity<MCXenoBombardComponent> entity, ref MCXenoBombardLaunchDoAfter args)
    {
        if (args.Handled)
            return;

        if (args.Cancelled)
        {
            _popup.PopupLocEntServer(entity, "mc-xeno-ability-bombard-launch-cancelled");
            return;
        }

        if (!_spit.TryGetShotCoordinates(entity, GetCoordinates(args.TargetCoordinates), out var origin, out var target) ||
            target.MapId != args.TargetMap ||
            !_glob.TryGetGlobId(entity.Owner, out var projectile))
            return;

        var action = GetEntity(args.ActionUid);
        if (!TryUseAction(entity, action))
            return;

        if (!_glob.TryRemoveGlobCount(entity.Owner, popup: true))
            return;

        args.Handled = true;

        _spit.Shoot(entity, origin, target, projectile, 1, Angle.Zero, entity.Comp.ProjectileSpeed, target: GetEntity(args.TargetUid));
        ApplyCooldownReduction(entity, action);
    }

    private bool CanLaunch(Entity<MCXenoBombardComponent> entity, EntityCoordinates coordinates)
    {
        if (!_glob.TryGetGlobId(entity.Owner, out _))
        {
            _popup.PopupLocEntServer(entity, "mc-xeno-ability-bombard-launch-cancelled-no-projectile", PopupType.MediumCaution);
            return false;
        }

        if (entity.Comp.MinDistance is { } min && (_transform.GetWorldPosition(entity) - _transform.ToWorldPosition(coordinates)).Length() < min)
        {
            _popup.PopupLocEntServer(entity, "mc-xeno-ability-bombard-launch-cancelled-too-close", PopupType.MediumCaution);
            return false;
        }

        if (!_glob.HasGlobCount(entity.Owner))
        {
            _popup.PopupLocEntServer(entity, "mc-xeno-ability-bombard-launch-cancelled-no-ammo", PopupType.MediumCaution);
            return false;
        }

        return true;
    }

    private void ApplyCooldownReduction(Entity<MCXenoBombardComponent> entity, EntityUid actionUid)
    {
        foreach (var actionEntity in RMCActions.GetActionsWithEvent<MCXenoBombardLaunchActionEvent>(entity))
        {
            if (actionEntity.Owner != actionUid || actionEntity.Comp.UseDelay is not { } delay)
                continue;

            var reduction = TimeSpan.FromSeconds(_glob.GetGlobCount(entity.Owner) * entity.Comp.AmmoCooldownReduction);
            Actions.SetCooldown((actionEntity, actionEntity), delay - reduction);
            return;
        }
    }
}
