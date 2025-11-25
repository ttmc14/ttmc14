using Content.Shared.Buckle.Components;
using Content.Shared.Emoting;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Pointing;
using Content.Shared.Speech;

namespace Content.Shared._MC.Xeno.Abilities.Banish;

public sealed partial class MCXenoBanishSystem
{
    private void InitializeBanished()
    {
        SubscribeLocalEvent<MCXenoBanishedComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<MCXenoBanishedComponent, SpeakAttemptEvent>(Cancel);
        SubscribeLocalEvent<MCXenoBanishedComponent, PointAttemptEvent>(Cancel);
        SubscribeLocalEvent<MCXenoBanishedComponent, EmoteAttemptEvent>(Cancel);
        SubscribeLocalEvent<MCXenoBanishedComponent, PickupAttemptEvent>(Cancel);
        SubscribeLocalEvent<MCXenoBanishedComponent, IsEquippingAttemptEvent>(Cancel);
        SubscribeLocalEvent<MCXenoBanishedComponent, IsUnequippingAttemptEvent>(Cancel);
        SubscribeLocalEvent<MCXenoBanishedComponent, AttackAttemptEvent>(Cancel);
        SubscribeLocalEvent<MCXenoBanishedComponent, UseAttemptEvent>(Cancel);
        SubscribeLocalEvent<MCXenoBanishedComponent, DropAttemptEvent>(Cancel);

        SubscribeLocalEvent<MCXenoBanishedComponent, InteractionAttemptEvent>(CancelInteraction);
        SubscribeLocalEvent<MCXenoBanishedComponent, ConsciousAttemptEvent>(CancelConscious);
        SubscribeLocalEvent<MCXenoBanishedComponent, UnbuckleAttemptEvent>(CancelUnbuckle);
    }

    private void OnShutdown(Entity<MCXenoBanishedComponent> entity, ref ComponentShutdown args)
    {
        if (Exists(entity.Comp.User) && TryComp<MCXenoBanishComponent>(entity.Comp.User, out var userBanishComponent))
        {
            userBanishComponent.Target = null;
            Dirty(entity.Comp.User, userBanishComponent);
        }

        _mcTransform.SetMapCoordinates(entity, entity.Comp.Position, unanchor: false);
    }

    private static void Cancel<T>(Entity<MCXenoBanishedComponent> _, ref T args) where T : CancellableEntityEventArgs
    {
        args.Cancel();
    }

    private static void CancelInteraction(Entity<MCXenoBanishedComponent> _, ref InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private static void CancelConscious(Entity<MCXenoBanishedComponent> _, ref ConsciousAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private static void CancelUnbuckle(Entity<MCXenoBanishedComponent> _, ref UnbuckleAttemptEvent args)
    {
        args.Cancelled = true;
    }
}
