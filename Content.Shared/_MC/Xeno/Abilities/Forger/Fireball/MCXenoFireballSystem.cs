using Content.Shared._MC.Xeno.Spit;
using Content.Shared.DoAfter;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Fireball;

public sealed class MCXenoFireballSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;

    [Dependency] private readonly MCSharedXenoSpitSystem _mcXenoSpit = null!;

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

        ActionStartUseDelay<MCXenoFireballActionEvent>(xeno);
    }
}
