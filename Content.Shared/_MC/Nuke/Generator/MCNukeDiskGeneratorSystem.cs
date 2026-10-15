using Content.Shared._MC.Nuke.Generator.Components;
using Content.Shared._MC.Nuke.Generator.Events;
using Content.Shared._MC.Nuke.Generator.UI;
using Content.Shared._MC.TacticalMap.TacticalMapIconSwitcher;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Power;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Nuke.Generator;

public sealed class MCNukeDiskGeneratorSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;

    [Dependency] private readonly MCTacticalMapIconSwitcherSystem _mcTacticalMapIconSwitcher = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCNukeDiskGeneratorRunningComponent, ComponentStartup>(OnRunningStart);
        SubscribeLocalEvent<MCNukeDiskGeneratorRunningComponent, ComponentRemove>(OnRunningEnd);

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

    private void OnRunningStart(Entity<MCNukeDiskGeneratorRunningComponent> entity, ref ComponentStartup args)
    {
        _mcTacticalMapIconSwitcher.SetIcon(entity.Owner, "on");
    }

    private void OnRunningEnd(Entity<MCNukeDiskGeneratorRunningComponent> entity, ref ComponentRemove args)
    {
        _mcTacticalMapIconSwitcher.ResetIcon(entity.Owner);
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
