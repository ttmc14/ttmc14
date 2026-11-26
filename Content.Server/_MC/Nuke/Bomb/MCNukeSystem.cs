using System.Linq;
using Content.Server.Chat.Managers;
using Content.Shared._MC.Areas;
using Content.Shared._MC.Nuke.Bomb.Components;
using Content.Shared._MC.Nuke.Bomb.Events;
using Content.Shared._MC.Nuke.Bomb.UI;
using Content.Shared._MC.Shuttle;
using Content.Shared._MC.TacticalMap.TacticalMapIconSwitcher;
using Content.Shared._RMC14.Dropship;
using Content.Shared._RMC14.Intel.Tech;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Announce;
using Content.Shared.ActionBlocker;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.UserInterface;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._MC.Nuke.Bomb;

public sealed partial class MCNukeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly ItemSlotsSystem _slots = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly ActivatableUISystem _activatable = null!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly MCTacticalMapIconSwitcherSystem _icons = null!;
    [Dependency] private readonly SharedXenoAnnounceSystem _hive = null!;
    [Dependency] private readonly MCAreasSystem _areas = null!;
    [Dependency] private readonly IChatManager _chat = null!;
    [Dependency] private readonly ActionBlockerSystem _blocker = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCNukeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MCNukeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MCNukeComponent, ActivatableUIOpenAttemptEvent>(OnOpenAttempt);
        SubscribeLocalEvent<MCNukeComponent, MCNukeDeployDoAfterEvent>(OnDeploy);
        SubscribeLocalEvent<MCNukeComponent, MCNukeDiskBuiMessage>(OnDisk);
        SubscribeLocalEvent<MCNukeComponent, MCNukeTimeBuiMessage>(OnTime);
        SubscribeLocalEvent<MCNukeComponent, MCNukeAnchorBuiMessage>(OnAnchor);
        SubscribeLocalEvent<MCNukeComponent, MCNukeSafetyBuiMessage>(OnSafety);
        SubscribeLocalEvent<MCNukeComponent, MCNukeActiveBuiMessage>(OnTimer);
        SubscribeLocalEvent<MCNukeComponent, AnchorStateChangedEvent>(OnAnchorChanged);
        SubscribeLocalEvent<MCNukeComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<MCNukeComponent, MCNukeDisarmDoAfterEvent>(OnDisarm);
        SubscribeLocalEvent<DropshipHijackStartEvent>(OnHijack);
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
    }

    /// <summary>Returns whether any planetary warhead is still counting down.</summary>
    public bool HasActiveCountdown() => GetNextCountdown() is not null;

    private void OnShutdown(Entity<MCNukeComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.DetonatesAt is null)
            return;

        ent.Comp.DetonatesAt = null;
        RefreshHiveAlerts();
        RaiseLocalEvent(new MCNukeDisarmedEvent());
    }

    private void OnMapInit(Entity<MCNukeComponent> ent, ref MapInitEvent args)
    {
        UpdateSlots(ent);
        UpdateAppearance(ent);
    }

    private void UpdateSlots(Entity<MCNukeComponent> ent)
    {
        foreach (var slot in ent.Comp.Slots)
        {
            _slots.SetLock(ent, slot, !ent.Comp.Deployed || ent.Comp.Disabled || ent.Comp.Exploded);
        }
    }

    private bool HasAuthorization(Entity<MCNukeComponent> ent)
    {
        return ent.Comp is { Deployed: true, Disabled: false, Exploded: false } &&
            ent.Comp.Slots.Count > 0 && ent.Comp.Slots.All(id => _slots.GetItemOrNull(ent, id) is not null);
    }

    private void OnOpenAttempt(Entity<MCNukeComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (HasComp<XenoComponent>(args.User) || ent.Comp.Disabled)
        {
            args.Cancel();
            return;
        }

        if (ent.Comp.Deployed)
            return;

        args.Cancel();
        StartInteraction(ent, args.User, ent.Comp.DeployTime, new MCNukeDeployDoAfterEvent());
    }

    private void StartInteraction(EntityUid target, EntityUid user, TimeSpan duration, DoAfterEvent ev)
    {
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, duration, ev, target, target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            DistanceThreshold = 1.5f,
            CancelDuplicate = false,
            RequireCanInteract = ev is not MCNukeDisarmDoAfterEvent,
        });
    }

    private void OnDeploy(Entity<MCNukeComponent> ent, ref MCNukeDeployDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || ent.Comp.Deployed || ent.Comp.Disabled || IsOnShuttle(ent))
            return;

        args.Handled = true;
        if (!Transform(ent).Anchored && !_transform.AnchorEntity(ent))
            return;

        ent.Comp.Deployed = true;
        Dirty(ent);
        UpdateSlots(ent);
        UpdateAppearance(ent);
        _activatable.InteractUI(args.User, ent);
    }

    private bool IsOnShuttle(EntityUid uid)
    {
        var grid = Transform(uid).GridUid;
        return HasComp<MCShuttleComponent>(grid) || HasComp<DropshipComponent>(grid);
    }

    private void OnDisk(Entity<MCNukeComponent> ent, ref MCNukeDiskBuiMessage args)
    {
        if (!ent.Comp.Deployed || ent.Comp.Disabled || ent.Comp.Exploded ||
            !ent.Comp.Slots.Contains(args.Slot) || !_slots.TryGetSlot(ent, args.Slot, out var slot))
            return;

        if (slot.HasItem)
        {
            _slots.TryEjectToHands(ent, slot, args.Actor);
            return;
        }

        _slots.TryInsertFromHand(ent, slot, args.Actor);
    }

    private void OnTime(Entity<MCNukeComponent> ent, ref MCNukeTimeBuiMessage args)
    {
        if (!HasAuthorization(ent) || ent.Comp.DetonatesAt is not null || args.Seconds is not (-10 or -5 or 5 or 10))
            return;

        var seconds = ent.Comp.Time.TotalSeconds + args.Seconds;
        ent.Comp.Time = TimeSpan.FromSeconds(Math.Clamp(seconds, ent.Comp.TimeMin.TotalSeconds, ent.Comp.TimeMax.TotalSeconds));
        Dirty(ent);
    }

    private void OnAnchor(Entity<MCNukeComponent> ent, ref MCNukeAnchorBuiMessage args)
    {
        if (!HasAuthorization(ent) || IsOnShuttle(ent))
            return;

        if (Transform(ent).Anchored)
        {
            _transform.Unanchor(ent);
            return;
        }

        _transform.AnchorEntity(ent);
    }

    private void OnAnchorChanged(Entity<MCNukeComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
        {
            StopCountdown(ent);
        }
    }

    private void OnSafety(Entity<MCNukeComponent> ent, ref MCNukeSafetyBuiMessage args)
    {
        if (!HasAuthorization(ent))
            return;

        ent.Comp.Safety = !ent.Comp.Safety;
        if (ent.Comp.Safety)
        {
            StopCountdown(ent);
        }

        Dirty(ent);
    }

    private void OnTimer(Entity<MCNukeComponent> ent, ref MCNukeActiveBuiMessage args)
    {
        if (!HasAuthorization(ent) || ent.Comp.Safety || !Transform(ent).Anchored || IsOnShuttle(ent))
            return;

        if (ent.Comp.DetonatesAt is not null)
        {
            StopCountdown(ent);
            return;
        }

        ent.Comp.DetonatesAt = _timing.CurTime + ent.Comp.Time;
        Dirty(ent);
        UpdateAppearance(ent);
        _icons.SetIcon(ent.Owner, "on");
        RefreshHiveAlerts();
        _hive.AnnounceAll(ent, Loc.GetString("mc-nuke-hive-start", ("area", _areas.GetAreaName(ent)),
            ("seconds", (int) ent.Comp.Time.TotalSeconds)));
    }

    private void OnAttacked(Entity<MCNukeComponent> ent, ref AttackedEvent args)
    {
        if (ent.Comp.DetonatesAt is null || args.Used != args.User || !HasComp<XenoComponent>(args.User))
            return;

        StartInteraction(ent, args.User, ent.Comp.DisarmTime, new MCNukeDisarmDoAfterEvent());
    }

    private void OnDisarm(Entity<MCNukeComponent> ent, ref MCNukeDisarmDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || !_blocker.CanInteract(args.User, null) || ent.Comp.DetonatesAt is not { } end ||
            args.DoAfter.StartTime < end - ent.Comp.Time)
            return;

        args.Handled = true;
        StopCountdown(ent);
    }

    private void StopCountdown(Entity<MCNukeComponent> ent)
    {
        if (ent.Comp.DetonatesAt is null)
            return;

        ent.Comp.DetonatesAt = null;
        Dirty(ent);
        UpdateAppearance(ent);
        _icons.ResetIcon(ent.Owner);
        _chat.DispatchServerAnnouncement(Loc.GetString("mc-nuke-stopped"));
        RefreshHiveAlerts();
        RaiseLocalEvent(new MCNukeDisarmedEvent());
    }

    private void OnHijack(ref DropshipHijackStartEvent args)
    {
        var bombs = AllEntityQuery<MCNukeComponent>();
        while (bombs.MoveNext(out var uid, out var bomb))
        {
            bomb.Disabled = true;
            StopCountdown((uid, bomb));
            _transform.Unanchor(uid);
            UpdateSlots((uid, bomb));
            UpdateAppearance((uid, bomb));
            Dirty(uid, bomb);
        }
    }

    private void UpdateAppearance(Entity<MCNukeComponent> ent)
    {
        var state = ent.Comp switch
        {
            { Exploded: true } => "explode",
            { Disabled: true } => "base",
            { DetonatesAt: not null } => "on",
            { Deployed: true } => "off",
            _ => "base",
        };
        _appearance.SetData(ent, MCNukeVisuals.State, state);
    }

    public override void Update(float frameTime)
    {
        var bombs = EntityQueryEnumerator<MCNukeComponent>();
        while (bombs.MoveNext(out var uid, out var bomb))
        {
            if (bomb.DetonatesAt is not { } end || _timing.CurTime < end)
                continue;

            bomb.DetonatesAt = null;
            bomb.Exploded = true;
            bomb.Safety = true;
            Dirty(uid, bomb);
            UpdateSlots((uid, bomb));
            UpdateAppearance((uid, bomb));
            _icons.ResetIcon(uid);
            _chat.DispatchServerAnnouncement(Loc.GetString("mc-nuke-detonated"));
            RefreshHiveAlerts();
            RaiseLocalEvent(new RMCNukeAudioEvent());
            RaiseLocalEvent(new MCNukeExplodedEvent(uid));

            var map = Transform(uid).MapID;
            var mobs = EntityQueryEnumerator<MobStateComponent, TransformComponent>();
            while (mobs.MoveNext(out var victim, out var mob, out var transform))
            {
                if (transform.MapID == map && mob.CurrentState != MobState.Dead &&
                    !(TryComp<MCShuttleComponent>(transform.GridUid, out var shuttle) && shuttle.Evacuated))
                {
                    _mobState.ChangeMobState(victim, MobState.Dead, mob, uid);
                }
            }
        }
    }
}
