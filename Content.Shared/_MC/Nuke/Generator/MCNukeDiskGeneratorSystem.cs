using Content.Shared._MC.Nuke.Generator.Components;
using Content.Shared._MC.Nuke.Generator.Events;
using Content.Shared._MC.Nuke.Generator.UI;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Power;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Nuke.Generator;

public sealed class MCNukeDiskGeneratorSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCNukeDiskGeneratorComponent, MCNukeDiskGeneratorRunBuiMessage>(OnRunMessage);
        SubscribeLocalEvent<MCNukeDiskGeneratorComponent, MCNukeDiskGeneratorRunDoAfterEvent>(OnRunDoAfter);

        SubscribeLocalEvent<MCNukeDiskGeneratorRunningComponent, PowerChangedEvent>(OnRunningPowerChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCNukeDiskGeneratorComponent, MCNukeDiskGeneratorRunningComponent>();
        while (query.MoveNext(out var uid, out var generator, out var running))
        {
            SetOverall((uid, generator), FixedPoint2.Clamp(generator.CheckpointProgress + generator.StepSize * ((_timing.CurTime - running.StartTime) / generator.StepDuration), generator.OverallProgress, generator.CheckpointProgress + generator.StepSize));

            if (_timing.CurTime < running.StartTime + generator.StepDuration)
                continue;

            generator.CheckpointProgress += generator.StepSize;
            RemCompDeferred<MCNukeDiskGeneratorRunningComponent>(uid);
        }
    }

    private void OnRunMessage(Entity<MCNukeDiskGeneratorComponent> entity, ref MCNukeDiskGeneratorRunBuiMessage args)
    {
        if (HasComp<MCNukeDiskGeneratorRunningComponent>(entity))
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.Actor, entity.Comp.InteractionTime, new MCNukeDiskGeneratorRunDoAfterEvent(), entity, entity, entity));
    }

    private void OnRunDoAfter(Entity<MCNukeDiskGeneratorComponent> entity, ref MCNukeDiskGeneratorRunDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        if (entity.Comp.CheckpointProgress >= 1)
        {
            if (_net.IsServer)
                Spawn(entity.Comp.SpawnId, _transform.GetMapCoordinates(entity));

            return;
        }

        if (HasComp<MCNukeDiskGeneratorRunningComponent>(entity))
            return;

        var state = new MCNukeDiskGeneratorRunningComponent
        {
            StartTime = _timing.CurTime,
        };

        AddComp(entity, state, true);
        Dirty(entity, state);
    }

    private void OnRunningPowerChanged(Entity<MCNukeDiskGeneratorRunningComponent> entity, ref PowerChangedEvent args)
    {
        if (args.Powered)
            return;

        if (!TryComp<MCNukeDiskGeneratorComponent>(entity, out var comp))
            return;

        SetOverall((entity, comp), comp.CheckpointProgress);
        RemCompDeferred<MCNukeDiskGeneratorRunningComponent>(entity);
    }

    private void SetOverall(Entity<MCNukeDiskGeneratorComponent> entity, FixedPoint2 value)
    {
        entity.Comp.OverallProgress = value;
        Dirty(entity);

        _userInterface.SetUiState(entity.Owner, MCNukeDiskGeneratorUi.Key, new MCNukeDiskGeneratorOverallProgressBuiState(value));
    }
}
