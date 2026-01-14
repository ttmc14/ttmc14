using Content.Shared._MC.Popup;
using Content.Shared.Popups;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.Bombard;

public sealed partial class MCXenoBombardSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = null!;

    private void Popup(
        Entity<MCXenoBombardComponent> entity,
        LocId id,
        PopupType type = PopupType.Small)
    {
        _popup.PopupEntityServer(
            Loc.GetString(id),
            entity,
            entity,
            type
        );
    }
}
