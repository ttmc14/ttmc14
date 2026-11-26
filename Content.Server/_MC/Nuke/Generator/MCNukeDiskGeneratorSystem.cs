using Content.Shared._MC.Nuke.Generator.Components;
using Content.Shared._MC.Nuke.Generator.Events;
using Content.Shared._MC.Nuke.Generator.UI;
using Content.Shared._MC.TacticalMap.TacticalMapIconSwitcher;
using Content.Shared._RMC14.Dropship;
using Content.Shared._RMC14.Power;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.ActionBlocker;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.UserInterface;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server._MC.Nuke.Generator;

public sealed class MCNukeDiskGeneratorSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedRMCPowerSystem _power = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly MCTacticalMapIconSwitcherSystem _icons = null!;

    [Dependency] private readonly ActionBlockerSystem _blocker = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCNukeDiskGeneratorComponent, ActivatableUIOpenAttemptEvent>(OnOpenAttempt);
        SubscribeLocalEvent<MCNukeDiskGeneratorComponent, MCNukeDiskGeneratorRunBuiMessage>(OnRun);
        SubscribeLocalEvent<MCNukeDiskGeneratorComponent, MCNukeDiskGeneratorRunDoAfterEvent>(OnRunDoAfter);
        SubscribeLocalEvent<MCNukeDiskGeneratorComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<MCNukeDiskGeneratorComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<MCNukeDiskGeneratorComponent, MCNukeDiskGeneratorInterruptDoAfterEvent>(OnInterrupt);
        SubscribeLocalEvent<DropshipHijackStartEvent>(OnHijack);
    }

    private void OnOpenAttempt(Entity<MCNukeDiskGeneratorComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (ent.Comp.Disabled || HasComp<XenoComponent>(args.User))
        {
            args.Cancel();
        }
    }

    private void OnRun(Entity<MCNukeDiskGeneratorComponent> ent, ref MCNukeDiskGeneratorRunBuiMessage args)
    {
        if (ent.Comp.Disabled || !_power.IsPowered(ent) || ent.Comp.Operation is not null || ent.Comp.StepEndsAt is not null)
            return;

        var duration = ent.Comp.CheckpointProgress >= 1 ? ent.Comp.PrintTime : ent.Comp.InteractionTime;
        var interaction = new DoAfterArgs(EntityManager, args.Actor, duration, new MCNukeDiskGeneratorRunDoAfterEvent(), ent, ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            DistanceThreshold = 1.5f,
        };
        // Instant do-afters raise their completion event inside TryStartDoAfter.
        if (!_doAfter.TryStartDoAfter(interaction, out ent.Comp.Operation))
        {
            ent.Comp.Operation = null;
            return;
        }

        UpdateOperation(ent);
    }

    private void UpdateOperation(Entity<MCNukeDiskGeneratorComponent> ent)
    {
        ent.Comp.Busy = ent.Comp.Operation is not null;
        Dirty(ent);
    }

    private void OnRunDoAfter(Entity<MCNukeDiskGeneratorComponent> ent, ref MCNukeDiskGeneratorRunDoAfterEvent args)
    {
        if (args.Handled || ent.Comp.Operation != args.DoAfter.Id)
            return;

        args.Handled = true;
        ent.Comp.Operation = null;
        UpdateOperation(ent);
        if (args.Cancelled || ent.Comp.Disabled || !_power.IsPowered(ent))
            return;

        if (ent.Comp.CheckpointProgress >= 1)
        {
            SpawnNextToOrDrop(ent.Comp.SpawnId, ent);
            _audio.PlayPvs(ent.Comp.CompletionSound, ent);
            return;
        }

        ent.Comp.StepEndsAt = _timing.CurTime + ent.Comp.StepDuration;
        _icons.SetIcon(ent.Owner, "on");
    }

    private void OnPowerChanged(Entity<MCNukeDiskGeneratorComponent> ent, ref PowerChangedEvent args)
    {
        if (!args.Powered)
        {
            Interrupt(ent);
        }
    }

    private void OnAttacked(Entity<MCNukeDiskGeneratorComponent> ent, ref AttackedEvent args)
    {
        if (ent.Comp.Operation is null && ent.Comp.StepEndsAt is null || args.Used != args.User || !HasComp<XenoComponent>(args.User))
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, ent.Comp.InterruptTime,
            new MCNukeDiskGeneratorInterruptDoAfterEvent(), ent, ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            CancelDuplicate = false,
            RequireCanInteract = false,
            DistanceThreshold = 1.5f,
        });
    }

    private void OnInterrupt(Entity<MCNukeDiskGeneratorComponent> ent, ref MCNukeDiskGeneratorInterruptDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !_blocker.CanInteract(args.User, null))
            return;

        args.Handled = true;
        Interrupt(ent);
    }

    private void Interrupt(Entity<MCNukeDiskGeneratorComponent> ent)
    {
        if (ent.Comp.Operation is null && ent.Comp.StepEndsAt is null)
            return;

        if (_doAfter.IsRunning(ent.Comp.Operation))
        {
            _doAfter.Cancel(ent.Comp.Operation);
        }

        ent.Comp.Operation = null;
        ent.Comp.StepEndsAt = null;
        UpdateOperation(ent);
        _icons.ResetIcon(ent.Owner);
        _popup.PopupEntity(Loc.GetString("mc-nuke-generator-interrupted"), ent);
    }

    private void OnHijack(ref DropshipHijackStartEvent args)
    {
        var generators = AllEntityQuery<MCNukeDiskGeneratorComponent>();
        while (generators.MoveNext(out var uid, out var generator))
        {
            generator.Disabled = true;
            Interrupt((uid, generator));
            Dirty(uid, generator);
        }
    }

    public override void Update(float frameTime)
    {
        var generators = EntityQueryEnumerator<MCNukeDiskGeneratorComponent>();
        while (generators.MoveNext(out var uid, out var generator))
        {
            if (generator.Operation is { } operation && !_doAfter.IsRunning(operation))
            {
                generator.Operation = null;
                UpdateOperation((uid, generator));
            }

            if (generator.StepEndsAt is not { } end || _timing.CurTime < end)
                continue;

            generator.StepEndsAt = null;
            generator.CheckpointProgress = FixedPoint2.Min(1, generator.CheckpointProgress + generator.StepSize);
            Dirty(uid, generator);
            _icons.ResetIcon(uid);
            _audio.PlayPvs(generator.CompletionSound, uid);
            var message = generator.CheckpointProgress >= 1 ? "mc-nuke-generator-ready" : "mc-nuke-generator-restart";
            _popup.PopupEntity(Loc.GetString(message), uid);
        }
    }
}
