using Content.Shared.Popups;

namespace Content.Shared._MC.Popup;

public static class SharedPopupSystemExtensions
{
    public static void PopupEntServer(this SharedPopupSystem popupSystem,
        string? message,
        EntityUid uid,
        PopupType type = PopupType.Small)
    {
        if (popupSystem.Net.IsClient)
            return;

        popupSystem.PopupEntity(message, uid, uid, type);
    }

    public static void PopupEntServer(this SharedPopupSystem popupSystem,
        string? message,
        EntityUid uid,
        EntityUid recipient,
        PopupType type = PopupType.Small)
    {
        if (popupSystem.Net.IsClient)
            return;

        popupSystem.PopupEntity(message, uid, recipient, type);
    }

    public static void PopupLocEntServer(this SharedPopupSystem popupSystem,
        EntityUid? uid,
        LocId id,
        PopupType type = PopupType.Small)
    {
        if (popupSystem.Net.IsClient || uid is null)
            return;

        popupSystem.PopupEntServer(
            Loc.GetString(id),
            uid.Value,
            uid.Value,
            type
        );
    }
}
