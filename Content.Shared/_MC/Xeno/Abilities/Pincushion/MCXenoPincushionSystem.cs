using Content.Shared._MC.Xeno.Spit;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Projectiles;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Pincushion;

public sealed class MCXenoPincushionSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly MCSharedXenoSpitSystem _mcXenoSpit = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPincushionComponent, MCXenoPincushionActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoPincushionComponent, MCXenoPincushionDoAfterEvent>(OnPincushionDoAfter);
    }

    private void OnAction(Entity<MCXenoPincushionComponent> entity, ref MCXenoPincushionActionEvent args)
    {
        if (args.Handled)
            return;

        if (!RMCActions.CanUseActionPopup(entity, args.Action, entity))
            return;

        var ev = new MCXenoPincushionDoAfterEvent(GetNetCoordinates(args.Target), GetNetEntity(args.Action), GetNetEntity(args.Entity));
        var doAfter = new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, ev, entity)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnPincushionDoAfter(Entity<MCXenoPincushionComponent> xeno, ref MCXenoPincushionDoAfterEvent args)
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

        StartUseDelay<MCXenoPincushionActionEvent>(xeno); // 5 secs
    }
}
