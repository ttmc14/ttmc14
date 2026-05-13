using System.Linq;
using Content.Shared._MC.Areas;
using Content.Shared._MC.Chat;
using Content.Shared._MC.Damage.Integrity.Systems;
using Content.Shared._MC.Engineering.Deploy;
using Content.Shared._MC.Engineering.Deploy.Components;
using Content.Shared._MC.Engineering.Deploy.Events;
using Content.Shared._MC.Engineering.Sentries.Components;
using Content.Shared._RMC14.Interaction;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Interaction;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Engineering.Sentries;

public sealed class MCSentrySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = null!;

    [Dependency] private readonly EntityLookupSystem _entityLookup = null!;

    [Dependency] private readonly MCAreasSystem _mcArea = null!;
    [Dependency] private readonly MCDeploySystem _mcDeploy = null!;
    [Dependency] private readonly MCIntegritySystem _mcIntegrity = null!;
    [Dependency] private readonly MCSharedRadioSystem _mcRadio = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCSentryComponent, CombatModeShouldHandInteractEvent>(OnShouldInteract);
        SubscribeLocalEvent<MCSentryComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<MCSentryComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<MCSentryComponent, MCDeployChangedStateEvent>(OnDeployChangedState);
        SubscribeLocalEvent<MCSentryComponent, MCDeployAttemptEvent>(OnDeployAttempt);
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
        switch (args.State)
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

    private void OnDeployAttempt(Entity<MCSentryComponent> entity, ref MCDeployAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        args.Cancelled = _entityLookup.GetEntitiesInRange<MCSentryComponent>(args.Coordinates, entity.Comp.DefenseCheckRange)
            .Any(uid => entity != uid && _mcDeploy.Deployed(uid));
    }
}
