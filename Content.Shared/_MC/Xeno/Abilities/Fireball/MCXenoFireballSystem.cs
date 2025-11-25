using System.Numerics;
using Content.Shared._RMC14.Actions;
using Content.Shared._MC.Xeno.Spit;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Projectiles;
using Content.Shared._RMC14.Xenonids.GasToggle;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Shared._MC.Xeno.Abilities.Fireball;

public sealed class MCXenoFireballSystem : EntitySystem
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

    private void OnFireball(Entity<MCXenoFireballComponent> xeno, ref MCXenoFireballActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_rmcActions.TryUseAction(xeno, args.Action, xeno))
            return;

        args.Handled = true;

        if (!_xenoPlasma.HasPlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;

        _audio.PlayPvs(xeno.Comp.SoundPrepare, xeno);

        var ev = new MCXenoFireballDoAfterEvent(GetNetCoordinates(args.Target), GetNetEntity(args.Entity));
        var doAfter = new DoAfterArgs(EntityManager, xeno, xeno.Comp.Delay, ev, xeno)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnFireballDoAfter(Entity<MCXenoFireballComponent> xeno, ref MCXenoFireballDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        if (!_xenoPlasma.TryRemovePlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;

        _mcXenoSpit.Shoot(
            xeno,
            GetCoordinates(args.Coordinates),
            xeno.Comp.ProjectileId,
            xeno.Comp.Count,
            xeno.Comp.MaxDeviation,
            xeno.Comp.Speed,
            xeno.Comp.Sound,
            target: GetEntity(args.Entity)
        );

        foreach (var (actionId, action) in _rmcActions.GetActionsWithEvent<MCXenoFireballActionEvent>(xeno))
        {
            _actions.SetCooldown(actionId, xeno.Comp.Cooldown);
        }
    }
}
