using Content.Shared._RMC14.Actions;
using Content.Shared._MC.Xeno.Spit;
using Content.Shared._RMC14.Projectiles;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Spawners;

namespace Content.Shared._MC.Xeno.Abilities.Fireball;

public sealed class MCXenoFireballSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedXenoHiveSystem _hive = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly RMCProjectileSystem _rmcProjectile = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly MCSharedXenoSpitSystem _mcXenoSpit = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoFireballComponent, MCXenoFireballActionEvent>(OnFireball);
        SubscribeLocalEvent<MCXenoFireballComponent, MCXenoFireballDoAfterEvent>(OnFireballDoAfter);
    }

    private void OnFireball(Entity<MCXenoFireballComponent> entity, ref MCXenoFireballActionEvent args)
    {
        if (args.Handled)
            return;

        if (!RMCActions.CanUseActionPopup(entity, args.Action, entity))
            return;

        _audio.PlayPredicted(entity.Comp.SoundPrepare, entity, entity);

        var ev = new MCXenoFireballDoAfterEvent(GetNetCoordinates(args.Target), GetNetEntity(args.Action), GetNetEntity(args.Entity));
        var doAfter = new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, ev, entity)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnFireballDoAfter(Entity<MCXenoFireballComponent> xeno, ref MCXenoFireballDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        var action = GetEntity(args.Action);
        if (!RMCActions.TryUseAction(xeno.Owner, action, xeno))
            return;

        args.Handled = true;

        _mcXenoSpit.Shoot(
            xeno,
            GetCoordinates(args.Coordinates),
            xeno.Comp.ProjectileId,
            1,
            xeno.Comp.MaxDeviation,
            xeno.Comp.Speed,
            xeno.Comp.Sound,
            target: GetEntity(args.Entity)
        );

        StartUseDelay<MCXenoFireballActionEvent>(xeno);
    }
}
