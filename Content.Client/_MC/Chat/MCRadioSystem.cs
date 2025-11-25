using Content.Shared._MC.Chat;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Client._MC.Chat;

public sealed class MCRadioSystem : MCSharedRadioSystem
{
    public override void SendRadioMessage(EntityUid messageSource,
        string message,
        ProtoId<RadioChannelPrototype> channel,
        EntityUid radioSource,
        bool escapeMarkup = true)
    {
    }
}
