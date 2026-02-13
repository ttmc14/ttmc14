using Content.Shared._MC.Flammable;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Armor;
using Content.Shared._RMC14.Stun;
using Content.Shared._RMC14.Weapons.Melee;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

// ReSharper disable UseCollectionExpression
namespace Content.Shared._MC.Xeno.Abilities;

public abstract class MCXenoAbilitySystem : EntitySystem
{
    [Dependency] protected readonly INetManager Net = null!;

    /// <summary>
    /// Reference to the central actions system used for validating and consuming ability actions.
    /// Automatically injected by dependency resolution.
    /// </summary>
    [Dependency] protected readonly SharedRMCActionsSystem RMCActions = null!;
    [Dependency] protected readonly SharedRMCMeleeWeaponSystem RMCMelee = null!;
    [Dependency] protected readonly SharedXenoHiveSystem RMCXenoHive = null!;

    [Dependency] protected readonly SharedActionsSystem Actions = null!;
    [Dependency] protected readonly SharedColorFlashEffectSystem ColorFlash = null!;
    [Dependency] protected readonly SharedMeleeWeaponSystem MeleeWeapon = null!;

    [Dependency] protected readonly MCSharedFlammableSystem MCFlammable = null!;

    [Dependency] private readonly MobStateSystem _mobState = null!;

    protected bool TryUseAction(EntityUid uid, EntityUid actionUid, EntityUid? targetUid = null, bool affectOnStructures = false, bool affectOnDead = false, bool allowUseOnFire = true)
    {
        if (!ValidateTarget(uid, targetUid, affectOnStructures, affectOnDead))
            return false;

        if (MCFlammable.OnFire(uid) && !allowUseOnFire)
            return false;

        return RMCActions.TryUseAction(uid, actionUid, uid);
    }

    protected bool CanUseAction(EntityUid uid, EntityUid actionUid, EntityUid? targetUid = null, bool affectOnStructures = false, bool affectOnDead = false, bool allowUseOnFire = true)
    {
        if (!ValidateTarget(uid, targetUid, affectOnStructures, affectOnDead))
            return false;

        if (MCFlammable.OnFire(uid) && !allowUseOnFire)
            return false;

        return RMCActions.CanUseActionPopup(uid, actionUid, uid);
    }

    protected bool ValidateTarget(EntityUid uid, EntityUid? targetUid, bool affectOnStructures = false, bool affectOnDead = false)
    {
        if (targetUid is null)
            return true;

        if (!IsMob(targetUid.Value) && !affectOnStructures)
            return false;

        if (_mobState.IsDead(targetUid.Value) && !affectOnDead)
            return false;

        return !RMCXenoHive.FromSameHive(uid, targetUid.Value);
    }

    #region Effects

    protected void AnimateHit(EntityUid ownerUid, EntityUid targetUid, Color? color = null)
    {
        RMCMelee.DoLunge(ownerUid, targetUid);
        RaiseEffect(ownerUid, targetUid, color);
    }

    protected void RaiseEffect(EntityUid uid, Color? color = null)
    {
        RaiseEffect(uid, uid, color);
    }

    protected void RaiseEffect(EntityUid ownerUid, EntityUid targetUid, Color? color = null)
    {
        var filter = Filter.Pvs(targetUid, entityManager: EntityManager).RemoveWhereAttachedEntity(uid => uid == ownerUid);
        ColorFlash.RaiseEffect(color ?? Color.Red, new List<EntityUid> { targetUid }, filter);
    }

    #endregion

    #region Actions

    protected void ActionClearUseDelay<T>(EntityUid uid) where T : BaseActionEvent
    {
        foreach (var action in RMCActions.GetActionsWithEvent<T>(uid))
        {
            Actions.ClearCooldown((action, action));
            break;
        }
    }

    protected void ActionStartUseDelay<T>(EntityUid uid) where T : BaseActionEvent
    {
        foreach (var action in RMCActions.GetActionsWithEvent<T>(uid))
        {
            Actions.StartUseDelay((action, action));
            break;
        }
    }

    protected void ActionStartUseDelay<T>(EntityUid uid, EntityUid actionUid) where T : BaseActionEvent
    {
        foreach (var action in RMCActions.GetActionsWithEvent<T>(uid))
        {
            if (action.Owner != actionUid)
                continue;

            Actions.StartUseDelay((action, action));
            break;
        }
    }

    protected void ActionSetUseDelay<T>(EntityUid uid, TimeSpan? delay) where T : BaseActionEvent
    {
        foreach (var action in RMCActions.GetActionsWithEvent<T>(uid))
        {
            Actions.SetUseDelay((action, action), delay);
            break;
        }
    }

    protected void ActionSetUseDelay<T>(EntityUid uid, EntityUid actionUid, TimeSpan? delay) where T : BaseActionEvent
    {
        foreach (var action in RMCActions.GetActionsWithEvent<T>(uid))
        {
            if (action.Owner != actionUid)
                continue;

            Actions.SetUseDelay((action, action), delay);
            break;
        }
    }

    protected void ActionSetCooldown<T>(EntityUid uid, EntityUid actionUid, TimeSpan cooldown) where T : BaseActionEvent
    {
        foreach (var action in RMCActions.GetActionsWithEvent<T>(uid))
        {
            if (action.Owner != actionUid)
                continue;

            Actions.SetCooldown((action, action), cooldown);
            break;
        }
    }

    protected void ActionSetToggled<T>(EntityUid uid, bool toggled) where T : BaseActionEvent
    {
        foreach (var action in RMCActions.GetActionsWithEvent<T>(uid))
        {
            Actions.SetToggled((action, action), toggled);
            break;
        }
    }

    protected void ActionSetToggled<T>(EntityUid uid, EntityUid actionUid, bool toggled) where T : BaseActionEvent
    {
        foreach (var action in RMCActions.GetActionsWithEvent<T>(uid))
        {
            if (action.Owner != actionUid)
                continue;

            Actions.SetToggled((action, action), toggled);
            break;
        }
    }

    protected void ActionSetState<T>(EntityUid uid, string state) where T : BaseActionEvent
    {
        foreach (var action in RMCActions.GetActionsWithEvent<T>(uid))
        {
            if (action.Comp.Icon is not SpriteSpecifier.Rsi rsi)
                continue;

            Actions.SetIcon((action, action), new SpriteSpecifier.Rsi(rsi.RsiPath, state));
            break;
        }
    }


    protected void ActionSetIcon<T>(EntityUid uid, SpriteSpecifier? icon) where T : BaseActionEvent
    {
        foreach (var action in RMCActions.GetActionsWithEvent<T>(uid))
        {
            Actions.SetIcon((action, action), icon);
            break;
        }
    }

    protected SpriteSpecifier? ActionGetIcon<T>(EntityUid uid) where T : BaseActionEvent
    {
        foreach (var action in RMCActions.GetActionsWithEvent<T>(uid))
        {
            return action.Comp.Icon;
        }

        return null;
    }

    #endregion

    #region Utilities

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

    protected bool IsDead(EntityUid uid)
    {
        return _mobState.IsDead(uid);
    }

    protected bool IsMob(EntityUid uid)
    {
        return HasComp<MobStateComponent>(uid);
    }

    protected bool IsXeno(EntityUid uid)
    {
        return HasComp<XenoComponent>(uid);
    }

    protected bool IsBig(EntityUid uid)
    {
        return TryComp<RMCSizeComponent>(uid, out var sizeComponent) && sizeComponent.Size == RMCSizes.Big;
    }

    protected float GetDistance(EntityUid fromUid, EntityUid destinationUid)
    {
        return (Transform(fromUid).Coordinates - Transform(destinationUid).Coordinates).Position.Length();
    }

    #endregion

    #region Spawn

    protected EntityUid ServerSpawn(string? prototype, EntityCoordinates coordinates)
    {
        return Net.IsClient ? EntityUid.Invalid : Spawn(prototype, coordinates);
    }

    protected EntityUid ServerSpawn(string? prototype, MapCoordinates coordinates, Angle rotation = default)
    {
        return Net.IsClient ? EntityUid.Invalid : Spawn(prototype, coordinates, rotation: rotation);
    }

    protected EntityUid ServerSpawnAttachedTo(string? prototype, EntityUid uid)
    {
        return Net.IsClient ? EntityUid.Invalid : SpawnAttachedTo(prototype, Transform(uid).Coordinates);
    }

    protected EntityUid ServerSpawnAttachedTo(string? prototype, EntityCoordinates coordinates, Angle rotation = default)
    {
        return Net.IsClient ? EntityUid.Invalid : SpawnAttachedTo(prototype, coordinates, rotation: rotation);
    }

    #endregion

    #region Del

    protected void ServerQueueDel(EntityUid? uid)
    {
        QueueDel(uid);
    }

    #endregion

    #region Component

    protected void RemCompDeferredDelayed<T>(EntityUid uid, TimeSpan duration) where T : IComponent
    {
        Timer.Spawn(duration, () => { RemCompDeferred<T>(uid); });
    }

    #endregion
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
