using Content.Shared._MC.Medical.Defibrillator.Components;
using Content.Shared.Interaction;
using Content.Shared.Traits.Assorted;

namespace Content.Shared._MC.Medical.Defibrillator;

public sealed partial class MCDefibrillatorSystem : EntitySystem
{
    /// <inheritdoc />
    public override void Initialize()
    {
        SubscribeLocalEvent<MCDefibrillatorComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<MCDefibrillatorComponent, MCDefibrillatorApplyDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<MCDefibrillatorComponent> entity, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target is not { } target)
            return;

        args.Handled = TryStart(entity, target, args.User);
    }

    private void OnDoAfter(Entity<MCDefibrillatorComponent> entity, ref MCDefibrillatorApplyDoAfterEvent args)
    {
        if (args.Handled)
            return;

        if (args.Cancelled)
        {
            StopChargingAudio(entity);
            return;
        }

        if (args.Target is not { } target)
            return;

        if (!CanUse(entity, target, args.User))
            return;

        args.Handled = true;
        TryApply(entity, target, args.User);
    }

    private void SendMessage(Entity<MCDefibrillatorComponent> entity, string message)
    {
        // _chatManager.TrySendInGameICMessage(uid, Loc.GetString("defibrillator-rotten"), InGameICChatType.Speak, true);
    }

    private void HandleSpecialCases(Entity<MCDefibrillatorComponent> entity, EntityUid target, EntityUid user)
    {
        if (_rotting.IsRotten(target))
            SendMessage(entity, Loc.GetString("defibrillator-rotten"));

        if (TryComp<UnrevivableComponent>(target, out var unrevivable))
            SendMessage(entity, Loc.GetString(unrevivable.ReasonMessage));
    }
}
