using Content.Shared._MC.Popup;
using Content.Shared._MC.Xeno.Abilities.Boiler.CreateBomb;
using Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector;
using Content.Shared._MC.Xeno.Spit;
using Content.Shared.DoAfter;
using Content.Shared.Popups;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.Bombard;

public sealed partial class MCXenoBombardSystem : MCXenoAbilitySystem
{

    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly MCXenoGlobSystem _glob = null!;
    [Dependency] private readonly MCXenoReagentSelectorSystem _reagents = null!;
    [Dependency] private readonly MCSharedXenoSpitSystem _spit = null!;

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

        if (IsBusyDigging(entity))
            return;

        if (TryDigging(entity, args.Action))
            return;

        if (!CanLaunch(entity))
            return;

        StartLaunchDoAfter(entity, args);
    }

    private void OnLaunchDoAfter(Entity<MCXenoBombardComponent> entity, ref MCXenoBombardLaunchDoAfter args)
    {
        if (args.Handled)
            return;

        if (args.Cancelled)
        {
            Popup(entity, "mc-xeno-ability-bombard-launch-cancelled");
            return;
        }

        if (!TryConsumeAmmo(entity, out var ammo))
            return;

        var action = GetEntity(args.ActionUid);
        if (!TryUseAction(entity, action))
            return;

        args.Handled = true;

        LaunchProjectile(entity, args);
        ApplyCooldownReduction(entity, action, ammo);
    }

    private bool CanLaunch(Entity<MCXenoBombardComponent> entity)
    {
        if (_reagents.GetSmoke(entity.Owner) is null)
            return false;

        if (_glob.HasValue(entity.Owner, 1))
            return true;

        Popup(entity, "mc-xeno-ability-bombard-launch-no-ammo", PopupType.MediumCaution);
        return false;
    }

    private void StartLaunchDoAfter(
        Entity<MCXenoBombardComponent> entity,
        MCXenoBombardLaunchActionEvent args)
    {
        var ev = new MCXenoBombardLaunchDoAfter(args.Action, args.Target, args.Entity, EntityManager);
        var doAfter = new DoAfterArgs(
            EntityManager,
            entity,
            entity.Comp.LaunchDuration,
            ev,
            entity
        );

        _doAfter.TryStartDoAfter(doAfter);
    }

    private bool TryConsumeAmmo(Entity<MCXenoBombardComponent> entity, out int ammo)
    {
        ammo = _glob.GetValue(entity.Owner);
        if (ammo <= 0)
        {
            Popup(entity, "mc-xeno-ability-bombard-launch-no-ammo", PopupType.MediumCaution);
            return false;
        }

        _glob.AdjustValue(entity.Owner, -1);
        return true;
    }

    private void LaunchProjectile(
        Entity<MCXenoBombardComponent> entity,
        MCXenoBombardLaunchDoAfter args)
    {
        if (_reagents.GetSmoke(entity.Owner) is not {} projectile)
            return;

        _spit.Shoot(
            entity,
            GetCoordinates(args.TargetCoordinates),
            projectile,
            1,
            Angle.Zero,
            entity.Comp.ProjectileSpeed,
            target: GetEntity(args.TargetUid)
        );
    }

    private void ApplyCooldownReduction(
        Entity<MCXenoBombardComponent> entity,
        EntityUid usedAction,
        int ammo)
    {
        foreach (var actionEntity in RMCActions.GetActionsWithEvent<MCXenoBombardLaunchActionEvent>(entity))
        {
            if (actionEntity.Owner != usedAction)
                continue;

            var reduction = TimeSpan.FromSeconds(
                ammo * entity.Comp.AmmoCooldownReduction
            );

            Actions.SetCooldown(
                (actionEntity, actionEntity),
                (actionEntity.Comp.UseDelay ?? TimeSpan.Zero) - reduction
            );
            return;
        }
    }
}
