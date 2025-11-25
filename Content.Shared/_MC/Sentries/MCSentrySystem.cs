using Content.Shared._MC.Areas;
using Content.Shared._MC.Chat;
using Content.Shared._MC.Damage.Integrity.Systems;
using Content.Shared._MC.Deploy;
using Content.Shared._MC.Deploy.Events;
using Content.Shared._RMC14.Interaction;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Interaction;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Sentries;

public sealed class MCSentrySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    [Dependency] private readonly MCAreasSystem _mcArea = default!;
    [Dependency] private readonly MCIntegritySystem _mcIntegrity = default!;
    [Dependency] private readonly MCSharedRadioSystem _mcRadio = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCSentryComponent, CombatModeShouldHandInteractEvent>(OnShouldInteract);
        SubscribeLocalEvent<MCSentryComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<MCSentryComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<MCSentryComponent, MCDeployChangedStateEvent>(OnDeployChangedState);
    }

    private void OnShouldInteract(Entity<MCSentryComponent> entity, ref CombatModeShouldHandInteractEvent args)
    {
        args.Cancelled = true;
    }

    private void OnDamageChanged(Entity<MCSentryComponent> entity, ref DamageChangedEvent args)
    {
        if (!entity.Comp.AlertMode)
            return;

        if (!args.DamageIncreased)
            return;

        if (entity.Comp.AlertDamageNextTime > _gameTiming.CurTime)
            return;

        entity.Comp.AlertDamageNextTime = _gameTiming.CurTime + entity.Comp.AlertDamageDelay;

        var message = Loc.GetString("mc-sentry-damage-alert",
            ("name", MetaData(entity).EntityName),
            ("coordsMessage", _mcArea.GetAreaCoordsMessage(entity)),
            ("integrityMessage", _mcIntegrity.GetDamageMessage(entity.Owner, "Destruction")));

        _mcRadio.SendRadioMessage(entity, message, entity.Comp.AlertChannel, entity);
    }

    private void OnDestruction(Entity<MCSentryComponent> entity, ref DestructionEventArgs args)
    {
        var message = Loc.GetString("mc-sentry-destroyed-alert",
            ("name", MetaData(entity).EntityName),
            ("coordsMessage", _mcArea.GetAreaCoordsMessage(entity)));
        _mcRadio.SendRadioMessage(entity, message, entity.Comp.AlertChannel, entity);
    }

    private void OnDeployChangedState(Entity<MCSentryComponent> entity, ref MCDeployChangedStateEvent args)
    {
        switch (args.NewState)
        {
            case MCDeployState.Item:
                RemCompDeferred<MaxRotationComponent>(entity);
                break;

            case MCDeployState.Deployed:
                // _rmcInteraction.SetMaxRotation(sentry.Owner, angle, sentry.Comp.MaxDeviation);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
