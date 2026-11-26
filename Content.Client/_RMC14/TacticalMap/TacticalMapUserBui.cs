using Content.Shared._RMC14.TacticalMap;
using JetBrains.Annotations;

namespace Content.Client._RMC14.TacticalMap;

[UsedImplicitly]
public sealed class TacticalMapUserBui(EntityUid owner, Enum uiKey) : TacticalMapBui(owner, uiKey)
{
    protected override void Open()
    {
        base.Open();
        if (Window is null)
            return;

        Window.Wrapper.OnQueenEyeMove += position =>
        {
            if (SelectedMap is { } map)
            {
                SendMessage(new TacticalMapQueenEyeMoveMsg(map, position));
            }
        };
    }

    /// <inheritdoc/>
    public override void Refresh()
    {
        if (EntMan.TryGetComponent<TacticalMapUserComponent>(Owner, out var user))
        {
            UpdateControls(user.CanDraw, user.LastAnnounceAt, user.NextAnnounceAt);
        }
    }
}
