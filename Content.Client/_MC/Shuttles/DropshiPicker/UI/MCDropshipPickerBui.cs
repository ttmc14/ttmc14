using Content.Shared._MC.Shuttles.DropshiPicker.Components;
using Content.Shared._MC.Shuttles.DropshiPicker.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._MC.Shuttles.DropshiPicker.UI;

[UsedImplicitly]
public sealed class MCDropshipPickerBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private MCDropshipPickerWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCDropshipPickerWindow>();
        _window.Open();

        if (!EntMan.TryGetComponent<MCDropshipPickerComponent>(Owner, out var component))
            return;

        _window.Container.Children.Clear();
        foreach (var data in component.DropshipGrids)
        {
            var button = new Button
            {
                Text = data.Name,
                HorizontalExpand = true,
            };

            button.OnPressed += _ => SendMessage(new MCDropshipPickerSelectBuiMessage(data.Path.ToString()));

            _window.Container.AddChild(button);
        }
    }
}
