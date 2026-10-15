using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Pointing;
using Content.Shared.Slippery;
using Content.Shared.Speech;
using Content.Shared.Verbs;

namespace Content.Shared._MC.StatusEffects.Sleep;

public sealed partial class MCEffectSleepSystem
{
    private void InitializeAttempts()
    {
        /*
        SubscribeLocalEvent<MCEffectSleepComponent, SpeakAttemptEvent>(OnSpeak);
        SubscribeLocalEvent<MCEffectSleepComponent, CanSeeAttemptEvent>(OnSeeAttempt);
        SubscribeLocalEvent<MCEffectSleepComponent, PointAttemptEvent>(OnPointAttempt);
        SubscribeLocalEvent<MCEffectSleepComponent, SlipAttemptEvent>(OnSlip);
        SubscribeLocalEvent<MCEffectSleepComponent, ConsciousAttemptEvent>(OnConsciousAttempt);
        SubscribeLocalEvent<MCEffectSleepComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<MCEffectSleepComponent, GetVerbsEvent<AlternativeVerb>>(AddWakeVerb);
        SubscribeLocalEvent<MCEffectSleepComponent, InteractHandEvent>(OnInteractHand);
        */
    }

    private void OnInteractHand(Entity<MCEffectSleepComponent> entity, ref InteractHandEvent args)
    {
        throw new NotImplementedException();
    }
}
