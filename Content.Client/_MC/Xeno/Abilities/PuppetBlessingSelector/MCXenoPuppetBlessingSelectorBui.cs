using System.Numerics;
using JetBrains.Annotations;
using Content.Shared._MC.Xeno.Abilities.Puppeteer;
using Content.Client.UserInterface.Controls;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client._MC.Xeno.Abilities.PuppetBlessingSelector;

[UsedImplicitly]
public sealed class MCXenoPuppetBlessingSelectorBui : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IEyeManager _eye = default!;

    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;

    [ViewVariables]
    private MCXenoPuppetBlessingSelectorMenu? _menu;

    public MCXenoPuppetBlessingSelectorBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);

        _sprite = EntMan.System<SpriteSystem>();
        _transform = EntMan.System<TransformSystem>();
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<MCXenoPuppetBlessingSelectorMenu>();
        var parent = _menu.FindControl<RadialContainer>("Main");

        foreach (MCXenoPuppetBlessing blessing in Enum.GetValues(typeof(MCXenoPuppetBlessing)))
        {
            AddButton(blessing, parent);
        }

        var vpSize = _displayManager.ScreenSize;
        var pos = _inputManager.MouseScreenPosition.Position / vpSize;

        if (EntMan.TryGetComponent<EyeComponent>(Owner, out var eyeComp) &&
            eyeComp.Target != null)
            pos = _eye.WorldToScreen(_transform.GetMapCoordinates((EntityUid)eyeComp.Target).Position) / vpSize;

        else if (_player.LocalEntity is { } ent)
            pos = _eye.WorldToScreen(_transform.GetMapCoordinates(ent).Position) / vpSize;

        _menu.OpenCenteredAt(pos);
    }

    private void AddButton(MCXenoPuppetBlessing blessing, RadialContainer parent)
    {
        var name = blessing.ToString().ToLowerInvariant();

        var texture = new TextureRect
        {
            VerticalAlignment = Control.VAlignment.Center,
            HorizontalAlignment = Control.HAlignment.Center,
            Texture = _sprite.Frame0(new SpriteSpecifier.Rsi(new ResPath("/Textures/_RMC14/Interface/xeno_pheromones.rsi"), name)),
            TextureScale = new Vector2(2f, 2f),
        };

        var button = new RadialMenuTextureButton
        {
            StyleClasses = { "RadialMenuButton" },
            SetSize = new Vector2(64, 64),
            ToolTip = name,
        };

        button.OnButtonDown += _ => SendPredictedMessage(new MCXenoPuppetBlessingChosenBuiMsg(blessing));

        button.AddChild(texture);
        parent.AddChild(button);
    }
}
