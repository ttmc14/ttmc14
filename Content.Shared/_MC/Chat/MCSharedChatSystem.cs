namespace Content.Shared._MC.Chat;

public abstract class MCSharedChatSystem : EntitySystem
{
    public virtual void TrySendInGameICSpeakMessage(
        EntityUid source,
        string message,
        bool hideLog = false,
        string? nameOverride = null,
        bool checkRadioPrefix = true,
        bool ignoreActionBlocker = false,
        bool ignoreXenos = false
    )
    {
    }
}
