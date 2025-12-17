using Content.Shared._MC.Knockback;
using Content.Shared._MC.Stun;
using Content.Shared._MC.Xeno.Abilities.Agility;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.CameraShake;
using Content.Shared._RMC14.Stamina;
using Content.Shared._RMC14.Stun;
using Content.Shared._RMC14.Weapons.Melee;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Damage;
using Content.Shared.Jittering;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Fling;

public sealed class MCXenoFlingSystem : EntitySystem
{
    private static readonly LocId LocIdTargetDead = "mc-xeno-fling-target-dead";
    private static readonly LocId LocIdTargetNotAlive= "mc-xeno-fling-not-alive";
    private static readonly LocId LocIdTargetSameHive= "mc-xeno-fling-target-from-same-hive";

    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedJitteringSystem _jittering = default!;
    [Dependency] private readonly MCStunSystem _mcStun = default!;
    [Dependency] private readonly RMCCameraShakeSystem _cameraShake = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly RMCStaminaSystem _stamina = default!;
    [Dependency] private readonly MCKnockbackSystem _mcKnockback = default!;
    [Dependency] private readonly SharedRMCMeleeWeaponSystem _rmcMelee = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoFlingComponent, MCXenoFlingActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoFlingComponent> entity, ref MCXenoFlingActionEvent args)
    {
        if (!TryUse(entity, ref args))
            return;

        // TODO: empower
        const bool empowered = false;

        _audio.PlayPredicted(entity.Comp.Sound, entity, entity);
        _cameraShake.ShakeCamera(args.Target, 1, 1);
        _rmcMelee.DoLunge(entity, args.Target);

        var big = TryComp<RMCSizeComponent>(args.Target, out var sizeComponent) && sizeComponent.Size == RMCSizes.Big;
        var distance = big ? entity.Comp.KnockbackDistanceBig : entity.Comp.KnockbackDistance;

        if (empowered)
            distance *= entity.Comp.EmpowerMultiplier;

        _mcKnockback.KnockbackFrom(args.Target, entity, distance, entity.Comp.KnockbackSpeed);
    }

    private bool TryUse(Entity<MCXenoFlingComponent> entity, ref MCXenoFlingActionEvent args)
    {
        if (args.Handled)
            return false;

        if (!HasComp<MobStateComponent>(args.Target))
        {
            PopupClient(LocIdTargetNotAlive);
            return false;
        }

        if (_mobState.IsDead(args.Target))
        {
            PopupClient(LocIdTargetDead);
            return false;
        }

        if (_xenoHive.FromSameHive(entity.Owner, args.Target))
        {
            PopupClient(LocIdTargetSameHive);
            return false;
        }

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return false;

        RemComp<MCXenoAgilityActiveComponent>(entity);

        args.Handled = true;
        return true;

        void PopupClient(LocId message)
        {
            _popup.PopupClient(Loc.GetString(message), entity, entity);
        }
    }
}
