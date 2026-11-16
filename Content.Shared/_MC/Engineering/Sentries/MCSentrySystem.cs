using System.Linq;
using Content.Shared._MC.Engineering.Deploy;
using Content.Shared._MC.Engineering.Deploy.Events;
using Content.Shared._MC.Engineering.Sentries.Components;
using Content.Shared.Damage;
using Content.Shared.Interaction;

namespace Content.Shared._MC.Engineering.Sentries;

public sealed class MCSentrySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = null!;

    [Dependency] private readonly MCDeploySystem _mcDeploy = null!;
    [Dependency] private readonly MCSentryUISystem _mcSentryUi = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCSentryComponent, CombatModeShouldHandInteractEvent>(OnShouldInteract);
        SubscribeLocalEvent<MCSentryComponent, MCDeployAttemptEvent>(OnDeployAttempt);
        SubscribeLocalEvent<MCSentryComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<MCSentryComponent> entity, ref DamageChangedEvent args) =>
        _mcSentryUi.Refresh(entity.Owner);

    private static void OnShouldInteract(Entity<MCSentryComponent> entity, ref CombatModeShouldHandInteractEvent args) =>
        args.Cancelled = true;

    private void OnDeployAttempt(Entity<MCSentryComponent> entity, ref MCDeployAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        args.Cancelled = _entityLookup.GetEntitiesInRange<MCSentryComponent>(args.Coordinates, entity.Comp.DefenseCheckRange)
            .Any(uid => entity != uid && _mcDeploy.Deployed(uid));
    }
}
