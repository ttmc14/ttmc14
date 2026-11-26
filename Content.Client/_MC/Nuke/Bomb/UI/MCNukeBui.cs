using Content.Shared._MC.Nuke.Bomb.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._MC.Nuke.Bomb.UI;

[UsedImplicitly]
public sealed class MCNukeBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    protected override void Open()
    {
        base.Open();
        var window = this.CreateWindow<MCNukeWindow>();
        window.TimerButton.OnPressed += _ => SendMessage(new MCNukeActiveBuiMessage());
        window.AnchorButton.OnPressed += _ => SendMessage(new MCNukeAnchorBuiMessage());
        window.SafetyButton.OnPressed += _ => SendMessage(new MCNukeSafetyBuiMessage());
        window.SetDevice(Owner, SendMessage);
    }
}
