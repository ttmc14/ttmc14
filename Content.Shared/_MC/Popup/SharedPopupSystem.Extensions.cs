using Content.Shared.Popups;

namespace Content.Shared._MC.Popup;

public static class SharedPopupSystemExtensions
{
    public static void PopupEntityServer(this SharedPopupSystem popupSystem,
        string? message,
        EntityUid uid,
        PopupType type = PopupType.Small)
    {
        if (popupSystem.Net.IsClient)
            return;

        popupSystem.PopupEntity(message, uid, uid, type);
    }

    public static void PopupEntityServer(this SharedPopupSystem popupSystem,
        string? message,
        EntityUid uid,
        EntityUid recipient,
        PopupType type = PopupType.Small)
    {
        if (popupSystem.Net.IsClient)
            return;

        popupSystem.PopupEntity(message, uid, recipient, type);
    }
}
