using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Chat;

public abstract class MCSharedRadioSystem : EntitySystem
{
    public abstract void SendRadioMessage(EntityUid messageSource,
        string message,
        ProtoId<RadioChannelPrototype> channel,
        EntityUid radioSource,
        bool escapeMarkup = true);
}
