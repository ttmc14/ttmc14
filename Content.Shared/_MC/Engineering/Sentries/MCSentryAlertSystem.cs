using Content.Shared._MC.Areas;
using Content.Shared._MC.Chat;
using Content.Shared._MC.Damage.Integrity.Systems;
using Content.Shared._MC.Engineering.Sentries.Components;
using Content.Shared._MC.Engineering.Sentries.UI.Message;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Engineering.Sentries;

public sealed class MCSentryAlertSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly MCAreasSystem _mcArea = null!;
    [Dependency] private readonly MCSharedRadioSystem _mcRadio = null!;
    [Dependency] private readonly MCIntegritySystem _mcIntegrity = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCSentryAlertComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<MCSentryAlertComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<MCSentryAlertComponent, MCSentryAlertModeBuiMessage>(OnModeMessage);
    }

    private void OnDamageChanged(Entity<MCSentryAlertComponent> entity, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased)
            return;

        SendMessage(entity,
            Loc.GetString(entity.Comp.DamageLoc,
                ("name", MetaData(entity).EntityName),
                ("coordsMessage", _mcArea.GetAreaCoordsMessage(entity)),
                ("integrityMessage", _mcIntegrity.GetDamageMessage(entity.Owner, "Destruction"))
            ),
    true
        );
    }

    private void OnDestruction(Entity<MCSentryAlertComponent> entity, ref DestructionEventArgs args) =>
        SendMessage(entity,
            Loc.GetString(entity.Comp.DestroyedLoc,
                ("name", MetaData(entity).EntityName),
                ("coordsMessage", _mcArea.GetAreaCoordsMessage(entity))
            )
        );

    private void OnModeMessage(Entity<MCSentryAlertComponent> entity, ref MCSentryAlertModeBuiMessage args)
    {
        entity.Comp.AlertMode = args.Enabled;
        Dirty(entity);
    }

    private void SendMessage(Entity<MCSentryAlertComponent> entity, LocId locId, bool useDelay = false)
    {
        if (!entity.Comp.AlertMode)
            return;

        if (useDelay)
        {
            if (entity.Comp.AlertNext > _timing.CurTime)
                return;

            entity.Comp.AlertNext = _timing.CurTime + entity.Comp.AlertDelay;
        }

        _mcRadio.SendRadioMessage(entity, locId, entity.Comp.AlertChannel, entity);
    }
}
