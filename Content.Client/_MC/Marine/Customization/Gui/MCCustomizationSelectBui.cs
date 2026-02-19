using System.Linq;
using Content.Shared._MC.Marine.Customization;
using Content.Shared._MC.Marine.Customization.Gui;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._MC.Marine.Customization.Gui;

[UsedImplicitly]
public sealed class MCCustomizationSelectBui : BoundUserInterface
{
    [ViewVariables]
    private MCCustomizationSelectBuiWindow? _window;

    private Dictionary<string, MCCustomizationVariationData> _data = new();
    private NetEntity _targetUid;

    public MCCustomizationSelectBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCCustomizationSelectBuiWindow>();
    }

    protected override void UpdateState(BoundUserInterfaceState baseState)
    {
        base.UpdateState(baseState);

        if (_window is not { } window)
            return;

        if (baseState is not MCCustomizationBuiState state)
            return;

        _data = state.Data;
        _targetUid = state.TargetUid;

        // Populate
        window.Container.Children.Clear();
        foreach (var (key, variationData) in _data.OrderBy(x => x.Value.Group).ThenBy(x => x.Value.Name))
        {
            var button = new Button
            {
                Text = $"{variationData.Name}",
                HorizontalExpand = true,
                VerticalExpand = false,
            };

            button.OnPressed += _ =>
            {
                SendMessage(new MCCustomizationSelectBuiMessage(key, state.TargetUid));
            };

            window.Container.AddChild(button);
        }
    }
}
