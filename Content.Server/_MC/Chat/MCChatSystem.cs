using Content.Server.Chat.Systems;
using Content.Shared._MC.Chat;

namespace Content.Server._MC.Chat;

public sealed class MCChatSystem : MCSharedChatSystem
{
    [Dependency] private readonly ChatSystem _chat = null!;

    public override void TrySendInGameICSpeakMessage(EntityUid source,
        string message,
        bool hideLog = false,
        string? nameOverride = null,
        bool checkRadioPrefix = true,
        bool ignoreActionBlocker = false,
        bool ignoreXenos = false)
    {
        _chat.TrySendInGameICMessage(source, message, InGameICChatType.Speak, ChatTransmitRange.Normal, hideLog, null, null, nameOverride, checkRadioPrefix, ignoreActionBlocker, ignoreXenos);
    }
}
