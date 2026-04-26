using Content.Shared._MC.Line;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared._MC.Xeno.Abilities.General.TransferPlasma;

public sealed class MCXenoTransferPlasmaSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;

    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = null!;
    [Dependency] private readonly MCLineSystem _mcLine = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoTransferPlasmaComponent, MCXenoTransferPlasmaActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoTransferPlasmaComponent, MCXenoTransferPlasmaDoAfter>(OnActionDoAfter);
    }

    private void OnAction(Entity<MCXenoTransferPlasmaComponent> entity, ref MCXenoTransferPlasmaActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_mcXenoHive.FromSameHive(entity.Owner, args.Target))
            return;

        if (!_mcXenoPlasma.CanTransferPlasma(entity, args.Target, entity.Comp.Amount))
            return;

        var ev = new MCXenoTransferPlasmaDoAfter();
        var doAfter = new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, ev, entity, args.Target)
        {
            BreakOnMove = true,
            RequireCanInteract = false,
            DistanceThreshold = entity.Comp.Range,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
            return;

        if (_net.IsServer)
        {
            SpawnAttachedTo(entity.Comp.EffectId, entity.Owner.ToCoordinates());
            SpawnAttachedTo(entity.Comp.EffectId, args.Target.ToCoordinates());
        }

        _audio.PlayPredicted(entity.Comp.Sound, entity, entity);
    }

    private void OnActionDoAfter(Entity<MCXenoTransferPlasmaComponent> entity, ref MCXenoTransferPlasmaDoAfter args)
    {
        if (args.Handled || args.Cancelled || args.Target is not {} targetUid)
            return;

        _mcLine.SpawnEffect(entity.Comp.RayEffectId, entity.Owner.ToCoordinates(), targetUid.ToCoordinates());

        if (!_mcXenoPlasma.TryTransferPlasma(entity, targetUid, entity.Comp.Amount))
            return;

        _audio.PlayPredicted(entity.Comp.Sound, entity, entity);

        args.Handled = true;
        ActionStartUseDelay<MCXenoTransferPlasmaActionEvent>(entity);
    }
}
