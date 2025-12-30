using System.Diagnostics.CodeAnalysis;
using Content.Shared._MC.Spreader;
using Content.Shared._MC.Stun.Events;
using Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.EmitNeurogas;

// TODO: [MC] Use MCXenoAbilitySystem<TComponent, TEvent>
public sealed class MCXenoEmitNeurogasSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly MCXenoReagentSelectorSystem _mcXenoReagentSelector = null!;
    [Dependency] private readonly SharedXenoHiveSystem _rmcXenoHive = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoEmitNeurogasComponent, MCXenoEmitNeurogasActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoEmitNeurogasComponent, MCXenoEmitNeurogasDoAfterEvent>(OnActionDoAfter);

        SubscribeLocalEvent<MCXenoEmitNeurogasActiveComponent, MobStateChangedEvent>(OnActiveMobStateChanged);
        SubscribeLocalEvent<MCXenoEmitNeurogasActiveComponent, MCStaggerEvent>(OnActiveStagger);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoEmitNeurogasActiveComponent, MCXenoEmitNeurogasComponent>();
        while (query.MoveNext(out var uid, out var component, out var neurogasComponent))
        {
            if (component.ActivationTimeNext > _timing.CurTime)
                continue;

            var rangeModifier = component.Activations == 1 ? 1 : 0;

            component.ActivationTimeNext = _timing.CurTime + component.ActivationDelay;
            component.Activations--;

            if (component.Activations <= 0)
                RemCompDeferred<MCXenoEmitNeurogasActiveComponent>(uid);

            var smokeUid = ServerSpawn(component.SmokeId, _transform.GetMapCoordinates(uid));
            if (!smokeUid.Valid)
                continue;

            _rmcXenoHive.SetSameHive(uid, smokeUid);
            _audio.PlayPvs(neurogasComponent.Sound, Transform(smokeUid).Coordinates);

            var spreader = EnsureComp<MCEdgeSpreaderComponent>(smokeUid);
            spreader.Range = component.Range + rangeModifier;
            Dirty(uid, spreader);
        }
    }

    private void OnAction(Entity<MCXenoEmitNeurogasComponent> entity, ref MCXenoEmitNeurogasActionEvent args)
    {
        if (args.Handled)
            return;

        if (!CanUseAction(entity, out _))
            return;

        if (!RMCActions.CanUseActionPopup(entity, args.Action, entity))
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, new MCXenoEmitNeurogasDoAfterEvent(GetNetEntity(args.Action)), entity)
        {
            BreakOnMove = false,
        });
    }

    private void OnActionDoAfter(Entity<MCXenoEmitNeurogasComponent> entity, ref MCXenoEmitNeurogasDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (!CanUseAction(entity, out var smokeId))
            return;

        var action = GetEntity(args.Action);
        if (!RMCActions.TryUseAction(entity, action, entity))
            return;

        args.Handled = true;
        ActionStartUseDelay<MCXenoEmitNeurogasActionEvent>(entity,  action);

        Active(entity, smokeId.Value);
    }

    private void OnActiveMobStateChanged(Entity<MCXenoEmitNeurogasActiveComponent> entity, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        RemCompDeferred<MCXenoEmitNeurogasActiveComponent>(entity);
    }

    private void OnActiveStagger(Entity<MCXenoEmitNeurogasActiveComponent> ent, ref MCStaggerEvent args)
    {
        RemComp<MCXenoEmitNeurogasActiveComponent>(ent);
    }

    private bool CanUseAction(Entity<MCXenoEmitNeurogasComponent> entity, [NotNullWhen(true)] out EntProtoId? smokeId)
    {
        smokeId = null;

        if (HasComp<MCXenoEmitNeurogasActiveComponent>(entity))
            return false;

        smokeId = _mcXenoReagentSelector.GetSmoke(entity.Owner);
        return smokeId is not null;
    }

    private void Active(Entity<MCXenoEmitNeurogasComponent> entity, EntProtoId smokeId)
    {
        var activate = EnsureComp<MCXenoEmitNeurogasActiveComponent>(entity);
        activate.ActivationDelay = entity.Comp.Duration;
        activate.Activations = entity.Comp.Activations;
        activate.Range = entity.Comp.Range;
        activate.SmokeId = smokeId;
        Dirty(entity, activate);
    }
}
