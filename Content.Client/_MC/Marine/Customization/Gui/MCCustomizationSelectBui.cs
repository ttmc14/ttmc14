using System.Linq;
using System.Numerics;
using Content.Shared._MC.Marine.Customization;
using Content.Shared._MC.Marine.Customization.Gui;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client._MC.Marine.Customization.Gui;

[UsedImplicitly]
public sealed class MCCustomizationSelectBui : BoundUserInterface
{
    [ViewVariables]
    private MCCustomizationSelectBuiWindow? _window;

    private Dictionary<string, MCCustomizationVariationData> _data = new();
    private NetEntity _targetUid;

    private readonly SpriteSystem _sprite;

    public MCCustomizationSelectBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _sprite = EntMan.System<SpriteSystem>();
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

        window.Container.Children.Clear();

        foreach (var (key, variationData) in _data.OrderBy(x => x.Value.Group).ThenBy(x => x.Value.Name))
        {
            var button = new Button
            {
                HorizontalExpand = true,
                VerticalExpand = true,
                MinSize = new Vector2(100, 100),
                Margin = new Thickness(4),
                RectClipContent = true,
            };

            var icon = new TextureRect
            {
                Texture = _sprite.Frame0(new SpriteSpecifier.Rsi(variationData.Path, "icon")), // TODO: fuck hardcode
                Stretch = TextureRect.StretchMode.KeepAspectCentered,
                HorizontalExpand = true,
                VerticalExpand = true,
            };

            button.AddChild(icon);
            button.ToolTip = variationData.Name;
            button.OnPressed += _ =>
            {
                SendMessage(new MCCustomizationSelectBuiMessage(key, state.TargetUid));
            };

            window.Container.AddChild(button);
        }
    }
}
