using Content.Shared._MC.Knockback;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Emote;
using Content.Shared._RMC14.Weapons.Melee;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.HeadbuttCharge;

public sealed class MCXenoHeadbuttChargeSystem : MCXenoAbilitySystem<MCXenoHeadbuttChargeComponent, MCXenoHeadbuttChargeActionEvent>
{
    private static readonly ProtoId<TagPrototype> AcidSprayTag = "MCAcidSpray";

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly SharedRMCEmoteSystem _emote = default!;
    [Dependency] private readonly SharedRMCMeleeWeaponSystem _rmcMelee = default!;
    [Dependency] private readonly MCKnockbackSystem _mcKnockback = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedXenoHiveSystem _hive = default!;
    [Dependency] private readonly INetManager _net = default!;

    protected override bool AutoUse => false;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoHeadbuttChargeActiveComponent, ComponentStartup>(OnActiveStartup);
        SubscribeLocalEvent<MCXenoHeadbuttChargeActiveComponent, ComponentRemove>(OnActiveRemove);
        SubscribeLocalEvent<MCXenoHeadbuttChargeActiveComponent, RefreshMovementSpeedModifiersEvent>(OnActiveRefreshSpeedModifier);
        SubscribeLocalEvent<MCXenoHeadbuttChargeActiveComponent, StartCollideEvent>(OnActiveCollide);
        SubscribeLocalEvent<MCXenoHeadbuttChargeActiveComponent, MoveEvent>(OnActiveMove);

        SubscribeLocalEvent<MCXenoHeadbuttChargeComponent, MCXenoHeadbuttChargeDoAfterEvent>(OnUseDoAfter);
    }

    protected override bool CanUse(Entity<MCXenoHeadbuttChargeComponent> entity, EntityUid actionUid)
    {
        return base.CanUse(entity, actionUid) && !HasComp<MCXenoHeadbuttChargeActiveComponent>(entity);
    }

    private void OnActiveStartup(Entity<MCXenoHeadbuttChargeActiveComponent> entity, ref ComponentStartup args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(entity);
    }

    private void OnActiveRemove(Entity<MCXenoHeadbuttChargeActiveComponent> entity, ref ComponentRemove args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(entity);
    }

    private void OnActiveRefreshSpeedModifier(Entity<MCXenoHeadbuttChargeActiveComponent> entity, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(entity.Comp.SpeedMultiplier);
    }

    private void OnActiveCollide(Entity<MCXenoHeadbuttChargeActiveComponent> entity, ref StartCollideEvent args)
    {
        if (!entity.Comp.Collide)
            return;

        if (!HasComp<MobStateComponent>(args.OtherEntity) || _mobState.IsDead(args.OtherEntity))
            return;

        if (_hive.FromSameHive(entity.Owner, args.OtherEntity))
            return;

        if (entity.Comp.DamageMultiplier != 0)
        {
            _damageable.TryChangeDamage(args.OtherEntity, GetDamage(entity) * entity.Comp.DamageMultiplier, tool: entity);
            RaiseEffect(entity, args.OtherEntity);
        }

        if (entity.Comp.Knockback != 0)
            _mcKnockback.KnockbackFrom(args.OtherEntity, entity, entity.Comp.Knockback, entity.Comp.KnockbackSpeed);

        if (entity.Comp.Paralyze != TimeSpan.Zero)
            _stun.TryParalyze(args.OtherEntity, entity.Comp.Paralyze, true);

        _audio.PlayPredicted(entity.Comp.HitSound, entity, entity);
        _rmcMelee.DoLunge(entity, args.OtherEntity);

        RemComp<MCXenoHeadbuttChargeActiveComponent>(entity);
    }

    private void OnActiveMove(Entity<MCXenoHeadbuttChargeActiveComponent> entity, ref MoveEvent args)
    {
        var coordinates = Transform(entity).Coordinates;
        if (_transform.GetGrid(coordinates) is not { } gridId || !TryComp<MapGridComponent>(gridId, out var grid))
            return;

        var tile = _map.TileIndicesFor(gridId, grid, coordinates);
        if (entity.Comp.LastTurf is not null && entity.Comp.LastTurf == tile)
            return;

        entity.Comp.FootstepTurfAccumulator++;
        entity.Comp.LastTurf = tile;
        Dirty(entity);

        if (entity.Comp.FootstepTurfAccumulator >= MCXenoHeadbuttChargeActiveComponent.FootstepTurfsCount)
        {
            _audio.PlayPredicted(entity.Comp.FootstepSound, entity, entity);
            entity.Comp.FootstepTurfAccumulator = 0;
        }

        if (entity.Comp.TurfSpawnEntityId is not { } spawnEntityId)
            return;

        var anchored = _map.GetAnchoredEntitiesEnumerator(gridId, grid, tile);
        while (anchored.MoveNext(out var uid))
        {
            if (_tag.HasTag(uid.Value, AcidSprayTag))
                return;
        }

        if (_net.IsClient)
            return;

        var spawn = SpawnAtPosition(spawnEntityId, coordinates);
        _hive.SetSameHive(entity.Owner, spawn);
    }

    protected override void OnUse(Entity<MCXenoHeadbuttChargeComponent> entity, ref MCXenoHeadbuttChargeActionEvent args)
    {
        var ev = new MCXenoHeadbuttChargeDoAfterEvent(GetNetEntity(args.Action));
        var doAfter = new DoAfterArgs(EntityManager, entity, entity.Comp.ActivationDelay, ev, entity);

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnUseDoAfter(Entity<MCXenoHeadbuttChargeComponent> entity, ref MCXenoHeadbuttChargeDoAfterEvent args)
    {
        var actionUid = GetEntity(args.Action);
        if (args.Handled || args.Cancelled || !TryUse(entity, actionUid))
            return;

        if (!TryComp<InstantActionComponent>(actionUid, out var instantActionComponent) || instantActionComponent.Event is not MCXenoHeadbuttChargeActionEvent ev)
            return;

        StartUseDelay(entity, actionUid);

        _emote.TryEmoteWithChat(entity, ev.ActivationEmote, forceEmote: true);

        var component = new MCXenoHeadbuttChargeActiveComponent
        {
            Collide = ev.Collide,
            Knockback = ev.Knockback,
            KnockbackSpeed = ev.KnockbackSpeed,
            Paralyze = ev.Paralyze,
            HitSound = ev.HitSound,
            DamageMultiplier = ev.DamageMultiplier,
            SpeedMultiplier = ev.SpeedMultiplier,
            TurfSpawnEntityId = ev.TurfSpawnEntityId,
            FootstepSound = ev.FootstepSound,
        };

        AddComp(entity, component, true);
        Dirty(entity, component);

        Timer.Spawn(ev.Duration,
            () =>
            {
                RemComp<MCXenoHeadbuttChargeActiveComponent>(entity);
            }
        );
    }
}
