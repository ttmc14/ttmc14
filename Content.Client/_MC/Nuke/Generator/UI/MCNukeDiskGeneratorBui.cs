using Content.Shared._MC.Nuke.Generator.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._MC.Nuke.Generator.UI;

[UsedImplicitly]
public sealed class MCNukeDiskGeneratorBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    protected override void Open()
    {
        base.Open();
        var window = this.CreateWindow<MCNukeDiskGeneratorWindow>();
        window.SetGenerator(Owner);
        window.RunButton.OnPressed += _ => SendMessage(new MCNukeDiskGeneratorRunBuiMessage());
    }
}
