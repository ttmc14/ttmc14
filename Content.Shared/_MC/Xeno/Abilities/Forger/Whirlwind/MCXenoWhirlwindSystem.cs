using Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind.Components;
using Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind.Events;
using Content.Shared.DoAfter;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind;

public sealed class MCXenoWhirlwindSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly MCXenoWhirlwindProjectileSystem _whirlwind = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoWhirlwindComponent, MCXenoWhirlwindActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoWhirlwindComponent, MCXenoWhirlwindDoAfterEvent>(OnDoAfter);
    }

    private void OnAction(Entity<MCXenoWhirlwindComponent> entity, ref MCXenoWhirlwindActionEvent args)
    {
        if (args.Handled)
            return;

        if (!RMCActions.CanUseActionPopup(entity, args.Action, entity))
            return;

        var ev = new MCXenoWhirlwindDoAfterEvent(args.Action, args.Target, EntityManager);
        var doAfter = new DoAfterArgs(EntityManager, entity, entity.Comp.ProjectileDelay, ev, entity)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfter(Entity<MCXenoWhirlwindComponent> entity, ref MCXenoWhirlwindDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        var actionUid = GetEntity(args.ActionUid);

        if (!RMCActions.TryUseAction(entity, actionUid, entity))
            return;

        args.Handled = true;
        _audio.PlayPredicted(entity.Comp.Sound, entity, entity);

        if (Net.IsServer)
            _whirlwind.Launch(entity.Comp.ProjectileId, entity, GetCoordinates(args.Coordinates));

        ActionStartUseDelay<MCXenoWhirlwindActionEvent>(entity, actionUid);
    }
}
