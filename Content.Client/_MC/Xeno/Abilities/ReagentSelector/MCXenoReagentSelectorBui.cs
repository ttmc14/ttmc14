using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector;
using Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector.UI;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._MC.Xeno.Abilities.ReagentSelector;

[UsedImplicitly]
public sealed class MCXenoReagentSelectorBui : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = null!;
    [Dependency] private readonly IInputManager _inputManager = null!;
    [Dependency] private readonly IPlayerManager _player = null!;
    [Dependency] private readonly IEyeManager _eye = null!;

    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;

    [ViewVariables]
    private MCXenoReagentSelectorMenu? _radialMenu;

    public MCXenoReagentSelectorBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);

        _sprite = EntMan.System<SpriteSystem>();
        _transform = EntMan.System<TransformSystem>();
    }

    protected override void Open()
    {
        base.Open();

        _radialMenu = this.CreateWindow<MCXenoReagentSelectorMenu>();
        var parent = _radialMenu.FindControl<RadialContainer>("Main");

        if (EntMan.TryGetComponent<MCXenoReagentSelectorComponent>(Owner, out var component))
        {
            foreach (var (key, entry) in component.Entries)
            {
                AddButton(key, entry, parent);
            }
        }

        var vpSize = _displayManager.ScreenSize;
        var pos = _inputManager.MouseScreenPosition.Position / vpSize;

        if (EntMan.TryGetComponent<EyeComponent>(Owner, out var eyeComp) && eyeComp.Target is not null)
            pos = _eye.WorldToScreen(_transform.GetMapCoordinates((EntityUid) eyeComp.Target).Position) / vpSize;
        else if (_player.LocalEntity is { } ent)
            pos = _eye.WorldToScreen(_transform.GetMapCoordinates(ent).Position) / vpSize;

        _radialMenu.OpenCenteredAt(pos);
    }

    private void AddButton(string key, MCXenoReagentSelectorComponent.Entry entry, RadialContainer parent)
    {
        var texture = new TextureRect
        {
            VerticalAlignment = Control.VAlignment.Center,
            HorizontalAlignment = Control.HAlignment.Center,
            Texture = _sprite.Frame0(entry.Sprite),
            TextureScale = new Vector2(2f, 2f),
        };

        var button = new RadialMenuTextureButton
        {
            StyleClasses = { "RadialMenuButton" },
            SetSize = new Vector2(64, 64),
        };

        button.OnButtonDown += _ => SendPredictedMessage(new MCXenoReagentSelectorBuiMsg(key));

        button.AddChild(texture);
        parent.AddChild(button);
    }
}
