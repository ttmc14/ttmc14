using Robust.Shared.Prototypes;

namespace Content.Shared._MC.ASRS.Events;

[ByRefEvent]
public record struct MCASRSBalanceChangedEvent(ProtoId<MCASRSAccountPrototype> Account, int Balance, int OldBalance);
