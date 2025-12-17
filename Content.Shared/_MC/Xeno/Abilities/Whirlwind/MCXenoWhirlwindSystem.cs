using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Chemistry;
using Content.Shared._RMC14.Entrenching;
using Content.Shared._RMC14.Line;
using Content.Shared._RMC14.Map;
using Content.Shared._RMC14.OnCollide;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Actions;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Content.Shared._MC.Xeno.Spit;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Whirlwind;

public sealed class MCXenoWhirlwindSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly LineSystem _line = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedOnCollideSystem _onCollide = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly RMCMapSystem _rmcMap = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MCSharedXenoSpitSystem _mcXenoSpit = default!;
    [Dependency] private readonly SharedXenoHiveSystem _hive = default!;

    private EntityQuery<BarricadeComponent> _barricadeQuery;
    private EntityQuery<MCXenoWhirlwindComponent> _xenoSprayAcidQuery;

    public override void Initialize()
    {
        _barricadeQuery = GetEntityQuery<BarricadeComponent>();
        _xenoSprayAcidQuery = GetEntityQuery<MCXenoWhirlwindComponent>();

        SubscribeLocalEvent<MCXenoWhirlwindComponent, MCXenoWhirlwindActionEvent>(OnWhirlwindAction);
        SubscribeLocalEvent<MCXenoWhirlwindComponent, MCXenoWhirlwindDoAfter>(OnWhirlwindDoAfter);
    }

    private void OnWhirlwindAction(Entity<MCXenoWhirlwindComponent> xeno, ref MCXenoWhirlwindActionEvent args)
    {
        if (!_xenoPlasma.HasPlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;

        var target = GetNetCoordinates(args.Target);

        var xenoCoords = _transform.GetMoverCoordinates(xeno);

        var length = (target.Position - xenoCoords.Position).Length();

        if (length > xeno.Comp.Range)
        {
            var direction = (target.Position - xenoCoords.Position).Normalized();
            var newTile = direction * xeno.Comp.Range;
            target = new NetCoordinates(GetNetEntity(args.Target.EntityId), xenoCoords.Position + newTile);
        }

        var ev = new MCXenoWhirlwindDoAfter(target);
        var doAfter = new DoAfterArgs(EntityManager, xeno, xeno.Comp.DoAfter, ev, xeno) { BreakOnMove = true };
        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnWhirlwindDoAfter(Entity<MCXenoWhirlwindComponent> xeno, ref MCXenoWhirlwindDoAfter args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;
        if (!_xenoPlasma.TryRemovePlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;

        if (_net.IsClient)
            return;

        foreach (var action in _rmcActions.GetActionsWithEvent<MCXenoWhirlwindActionEvent>(xeno))
        {
            _actions.StartUseDelay(action.AsNullable());
        }

        var start = xeno.Owner.ToCoordinates();
        var end = GetCoordinates(args.Coordinates);
        var tiles = _line.DrawLine(start, end, xeno.Comp.Delay, out var blocker);
        var active = EnsureComp<MCXenoWhirlwindSprayingComponent>(xeno);
        active.Blocker = blocker;
        active.Fire = xeno.Comp.Fire;
        active.Spawn = tiles;

        _mcXenoSpit.Shoot(
            xeno,
            GetCoordinates(args.Coordinates),
            xeno.Comp.ProjectileId,
            xeno.Comp.Count,
            xeno.Comp.MaxDeviation,
            xeno.Comp.Speed,
            xeno.Comp.Sound
            //target: GetEntity(args.Entity)
        );

        Dirty(xeno, active);
    }

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var time = _timing.CurTime;
        var spraying = EntityQueryEnumerator<MCXenoWhirlwindSprayingComponent>();
        while (spraying.MoveNext(out var uid, out var active))
        {
            active.Chain ??= _onCollide.SpawnChain();
            for (var i = active.Spawn.Count - 1; i >= 0; i--)
            {
                var acid = active.Spawn[i];
                if (time < acid.At)
                    continue;

                var spawned = Spawn(active.Fire, acid.Coordinates);
                var splatter = EnsureComp<MCXenoWhirlwindSprayingComponent>(spawned);
                _hive.SetSameHive(uid, spawned);
                //splatter.Xeno = uid;
                Dirty(spawned, splatter);

                if (_xenoSprayAcidQuery.TryComp(uid, out var xenoSprayAcid))
                {
                    //var spray = new Entity<MCXenoWhirlwindSprayingComponent>(uid, xenoSprayAcid);

                    // Same tile
                    //TryAcid(spray, _rmcMap.GetAnchoredEntitiesEnumerator(spawned));

                    if (active.Spawn.Count <= 1 && active.Blocker != null)
                    {
                        //TryAcid(spray, active.Blocker.Value);
                        active.Blocker = null;
                        Dirty(uid, active);
                    }
                }

                _onCollide.SetChain(spawned, active.Chain.Value);

                active.Spawn.RemoveAt(i);
            }
        }
    }
}
