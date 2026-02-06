using Content.Shared._RMC14.Body;
using Content.Shared._RMC14.Hands;
using Content.Shared._RMC14.Marines.Skills;
using Content.Shared._RMC14.Temperature;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Timing;
using Content.Shared.Medical.Components;
using Content.Shared.Inventory;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._RMC14.Medical.Scanner;

public sealed partial class HealthScannerSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedEntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedRMCBloodstreamSystem _rmcBloodstream = default!;
    [Dependency] private readonly RMCHandsSystem _rmcHands = default!;
    [Dependency] private readonly SharedRMCTemperatureSystem _rmcTemperature = default!;
    [Dependency] private readonly SkillsSystem _skills = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HealthScannerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<HealthScannerComponent, DoAfterAttemptEvent<HealthScannerDoAfterEvent>>(OnDoAfterAttempt);
        SubscribeLocalEvent<HealthScannerComponent, HealthScannerDoAfterEvent>(OnDoAfter);
    }
    private void OnAfterInteract(Entity<HealthScannerComponent> scanner, ref AfterInteractEvent args)
    {
        if (TryComp(scanner, out MCHealthGlovesComponent? _))
        {
            if (!_inventory.TryGetSlotEntity(args.User, "gloves", out var equipped) || equipped != scanner.Owner)
            {
                return;
            }
        }

        if (!args.CanReach ||
            args.Target is not { } target ||
            !CanUseHealthScannerPopup(scanner, args.User, ref target))
        {
            return;
        }

        var delay = _skills.GetDelay(args.User, scanner);
        var ev = new HealthScannerDoAfterEvent();
        var doAfter = new DoAfterArgs(EntityManager, args.User, delay, ev, scanner, target, scanner)
        {
            BreakOnMove = true,
            AttemptFrequency = AttemptFrequency.EveryTick
        };

        if (delay > TimeSpan.Zero)
        {
            var name = Loc.GetString("zzzz-the", ("ent", target));
            _popup.PopupClient($"You start fumbling around with {name}...", target, args.User);
        }

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfterAttempt(Entity<HealthScannerComponent> ent, ref DoAfterAttemptEvent<HealthScannerDoAfterEvent> args)
    {
        var doAfter = args.DoAfter.Args;
        if (doAfter.Target is not { } target)
            return;

        if (!CanUseHealthScannerPopup(ent, doAfter.User, ref target))
        {
            args.Cancel();
            return;
        }

        var userCoords = Transform(doAfter.User).Coordinates;
        if (!_transform.InRange(userCoords, args.DoAfter.UserPosition, doAfter.MovementThreshold))
            args.Cancel();
    }

    private void OnDoAfter(Entity<HealthScannerComponent> scanner, ref HealthScannerDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target)
            return;

        args.Handled = true;

        if (TryComp(scanner, out UseDelayComponent? useDelay))
            _useDelay.TryResetDelay((scanner, useDelay));

        scanner.Comp.Target = target;
        Dirty(scanner);

        _audio.PlayPredicted(scanner.Comp.Sound, scanner, args.User);
        _ui.OpenUi(scanner.Owner, HealthScannerUIKey.Key, args.User);

        UpdateUI(scanner);
    }

    /// <param name="scanner">The Health Scanner</param>
    /// <param name="user"> The entity using the Health Scanner</param>
    /// <param name="target">The entity being scanned by the Health Scanner. May be changed</param>
    /// <returns></returns>
    private bool CanUseHealthScannerPopup(Entity<HealthScannerComponent> scanner, EntityUid user, ref EntityUid target)
    {
        if (TryComp(scanner, out MCHealthGlovesComponent? _))
        {
            if (!_inventory.TryGetSlotEntity(user, "gloves", out var equipped) || equipped != scanner.Owner)
            {
                return false;
            }
        }

        SharedEntityStorageComponent? entityStorage = null;
        if (HasComp<HealthScannableContainerComponent>(target) && _entityStorage.ResolveStorage(target, ref entityStorage))
        {
            foreach (var entity in entityStorage.Contents.ContainedEntities)
            {
                if (HasComp<DamageableComponent>(entity) &&
                HasComp<MobStateComponent>(entity) &&
                HasComp<MobThresholdsComponent>(entity))
                {
                    target = entity;
                    break;
                }
            }
        }

        if (!HasComp<DamageableComponent>(target) ||
            !HasComp<MobStateComponent>(target) ||
            !HasComp<MobThresholdsComponent>(target))
        {
            return false;
        }

        if (TryComp(scanner, out UseDelayComponent? useDelay) &&
            _useDelay.IsDelayed((scanner, useDelay)))
        {
            return false;
        }

        var ev = new HealthScannerAttemptTargetEvent();
        RaiseLocalEvent(target, ref ev);
        if (ev.Cancelled)
        {
            if (ev.Popup != null)
                _popup.PopupClient(ev.Popup, target, user);

            return false;
        }

        return true;
    }

    public void UpdateUI(Entity<HealthScannerComponent> scanner, bool force = false)
    {
        if (scanner.Comp.Target is not { } target)
            return;

        if (TerminatingOrDeleted(target))
        {
            if (!TerminatingOrDeleted(scanner))
                _ui.CloseUi(scanner.Owner, HealthScannerUIKey.Key);

            scanner.Comp.Target = null;
            Dirty(scanner);
            return;
        }

        if (!force && !scanner.Comp.Force)
        {
            if (TryComp(scanner, out MCHealthGlovesComponent? _))
            {
                if (!_inventory.TryGetContainingSlot(scanner.Owner, out var slot) || slot == null || slot.Name != "gloves")
                    return;
            }
            else
            {
                var isHeld = _rmcHands.TryGetHolder(scanner, out _);
                if (!isHeld)
                {
                    if (!_inventory.TryGetContainingSlot(scanner.Owner, out var slot) || slot == null || slot.Name != "gloves")
                        return;
                }
            }
        }

        FixedPoint2 blood = 0;
        FixedPoint2 maxBlood = 0;
        if (_rmcBloodstream.TryGetBloodSolution(target, out var bloodstream))
        {
            blood = bloodstream.Volume;
            maxBlood = bloodstream.MaxVolume;
        }

        _rmcBloodstream.TryGetChemicalSolution(target, out _, out var chemicals);
        _rmcTemperature.TryGetCurrentTemperature(target, out var temperature);

        var bleeding = _rmcBloodstream.IsBleeding(target);
        var state = new HealthScannerBuiState(GetNetEntity(target), blood, maxBlood, temperature, chemicals, bleeding);

        _ui.SetUiState(scanner.Owner, HealthScannerUIKey.Key, state);
    }

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var time = _timing.CurTime;
        var scanners = EntityQueryEnumerator<HealthScannerComponent>();
        while (scanners.MoveNext(out var uid, out var active))
        {
            if (time < active.UpdateAt)
                continue;

            active.UpdateAt = time + active.UpdateCooldown;
            UpdateUI((uid, active));
        }
    }
}
