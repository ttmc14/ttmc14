using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Armor;
using Content.Shared._RMC14.Weapons.Melee;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities;

public abstract class MCXenoAbilitySystem : EntitySystem
{
    [Dependency] protected readonly INetManager Net = null!;

    /// <summary>
    /// Reference to the central actions system used for validating and consuming ability actions.
    /// Automatically injected by dependency resolution.
    /// </summary>
    [Dependency] protected readonly RMCActionsSystem RMCActions = null!;
    [Dependency] protected readonly SharedRMCMeleeWeaponSystem RMCMelee = null!;

    [Dependency] protected readonly SharedActionsSystem Actions = null!;
    [Dependency] protected readonly SharedColorFlashEffectSystem ColorFlash = null!;
    [Dependency] protected readonly SharedMeleeWeaponSystem MeleeWeapon = null!;

    protected DamageSpecifier GetDamage(EntityUid uid)
    {
        return MeleeWeapon.GetDamage(uid, uid);
    }

    protected int GetArmorPiercing(EntityUid uid)
    {
        return TryComp<CMArmorPiercingComponent>(uid, out var comp)
            ? comp.Amount
            : 0;
    }

    protected void AnimateHit(EntityUid ownerUid, EntityUid targetUid, Color? color = null)
    {
        RMCMelee.DoLunge(ownerUid, targetUid);
        RaiseEffect(ownerUid, targetUid, color);
    }

    protected void RaiseEffect(EntityUid ownerUid, EntityUid targetUid, Color? color = null)
    {
        var filter = Filter.Pvs(targetUid, entityManager: EntityManager).RemoveWhereAttachedEntity(uid => uid == ownerUid);
        ColorFlash.RaiseEffect(color ?? Color.Red, new List<EntityUid> { targetUid }, filter);
    }

    protected bool IsMob(EntityUid uid)
    {
        return HasComp<MobStateComponent>(uid);
    }

    protected void RemCompDeferredDelayed<T>(EntityUid uid, TimeSpan duration) where T : IComponent
    {
        Timer.Spawn(duration, () => { RemCompDeferred<T>(uid); });
    }

    protected void ClearUseDelay<T>(EntityUid uid) where T : BaseActionEvent
    {
        foreach (var action in RMCActions.GetActionsWithEvent<T>(uid))
        {
            Actions.ClearCooldown((action, action));
            break;
        }
    }

    protected void StartUseDelay<T>(EntityUid uid) where T : BaseActionEvent
    {
        foreach (var action in RMCActions.GetActionsWithEvent<T>(uid))
        {
            Actions.StartUseDelay((action, action));
            break;
        }
    }

    protected void StartUseDelay<T>(EntityUid uid, EntityUid actionUid) where T : BaseActionEvent
    {
        foreach (var action in RMCActions.GetActionsWithEvent<T>(uid))
        {
            if (action.Owner != actionUid)
                continue;

            Actions.StartUseDelay((action, action));
            break;
        }
    }

    protected EntityUid SpawnServer(string? prototype, EntityCoordinates coordinates)
    {
        return Net.IsClient ? EntityUid.Invalid : Spawn(prototype, coordinates);
    }

    protected EntityUid SpawnServer(string? prototype, MapCoordinates coordinates)
    {
        return Net.IsClient ? EntityUid.Invalid : Spawn(prototype, coordinates);
    }
}

/// <summary>
/// Base generic system for handling Xeno abilities.
/// Provides unified logic for validating and executing custom ability actions.
/// </summary>
/// <typeparam name="TComp">The component type required on the entity to receive the action.</typeparam>
/// <typeparam name="TAction">The type of the action event to handle (derived from <see cref="BaseActionEvent"/>).</typeparam>
public abstract class MCXenoAbilitySystem<TComp, TAction> : MCXenoAbilitySystem where TComp : IComponent where TAction : BaseActionEvent
{
    /// <summary>
    /// Determines whether the ability should automatically attempt to consume its action when it is triggered.
    /// When true, the action is immediately passed through <see cref="TryUse"/> which consumes it on success.
    /// When false, the action is only validated through <see cref="CanUse"/> and does not get consumed automatically.
    /// </summary>
    protected virtual bool AutoUse => true;

    /// <summary>
    /// Initializes the system and subscribes to the given action event type.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TComp, TAction>(OnAction);
    }

    /// <summary>
    /// Handles the action event for the given entity.
    /// If the event has already been handled, it does nothing.
    /// Otherwise, it first checks whether the action can be activated by calling <see cref="CanActivate"/>.
    /// If the validation succeeds, the ability effect is executed by calling <see cref="OnUse"/>.
    /// </summary>
    protected virtual void OnAction(Entity<TComp> entity, ref TAction args)
    {
        if (args.Handled)
            return;

        if (!CanActivate(entity, ref args))
            return;

        OnUse(entity, ref args);
    }

    /// <summary>
    /// Determines if the action can be activated, depending on the value of <see cref="AutoUse"/>.
    /// When AutoUse is true, it attempts to consume the action immediately with <see cref="TryUse"/>.
    /// When AutoUse is false, it only validates the action through <see cref="CanUse"/>.
    /// </summary>
    protected virtual bool CanActivate(Entity<TComp> entity, ref TAction args)
    {
        return AutoUse
            ? TryUse(entity, ref args)
            : CanUse(entity, ref args);
    }

    /// <summary>
    /// Checks whether the action can be used without actually consuming it.
    /// If the action is not usable, a feedback popup is shown to the player.
    /// This is useful when the ability requires certain conditions but should not immediately trigger.
    /// </summary>
    protected bool CanUse(Entity<TComp> entity, ref TAction args)
    {
        return CanUse(entity, args.Action);
    }

    protected virtual bool CanUse(Entity<TComp> entity, EntityUid actionUid)
    {
        return RMCActions.CanUseActionPopup(entity, actionUid, entity);
    }

    /// <summary>
    /// Attempts to consume and use the action right away.
    /// If successful, it applies cooldowns or deducts charges depending on the action type.
    /// If the action cannot be used, the method returns false and nothing is consumed.
    /// </summary>
    protected bool TryUse(Entity<TComp> entity, ref TAction args)
    {
        if (!TryUse(entity, args.Action))
            return false;

        args.Handled = true;
        return true;
    }

    protected virtual bool TryUse(Entity<TComp> entity, EntityUid actionUid)
    {
        if (!CanUse(entity, actionUid))
            return false;

        var ev = new RMCActionUseEvent(entity);
        RaiseLocalEvent(actionUid, ref ev);
        return true;
    }

    /// <summary>
    /// Defines the actual effect of the ability once validated.
    /// Must be implemented by derived systems to specify the ability's behavior.
    /// </summary>
    /// <param name="entity">The entity performing the ability.</param>
    /// <param name="args">The action event arguments.</param>
    protected abstract void OnUse(Entity<TComp> entity, ref TAction args);

    protected void StartUseDelay(Entity<TComp> entity, EntityUid actionUid)
    {
        foreach (var action in RMCActions.GetActionsWithEvent<TAction>(entity))
        {
            if (action.Owner != actionUid)
                continue;

            Actions.StartUseDelay((action, action));
            break;
        }
    }
}
