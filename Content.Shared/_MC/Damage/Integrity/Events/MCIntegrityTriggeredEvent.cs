using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Damage.Integrity.Events;

[ByRefEvent]
public readonly struct MCIntegrityTriggeredEvent
{
    public readonly ProtoId<MCIntegrityPrototype> IntegrityId;

    public MCIntegrityTriggeredEvent(ProtoId<MCIntegrityPrototype> integrityId)
    {
        IntegrityId = integrityId;
    }
}
