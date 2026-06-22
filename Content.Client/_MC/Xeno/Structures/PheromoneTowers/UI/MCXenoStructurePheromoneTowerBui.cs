using System.Numerics;
using Content.Client._RMC14.Xenonids.Pheromones;
using Content.Client.UserInterface.Controls;
using Content.Shared._MC.Xeno.Constructions.PheromoneTowers.UI;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Pheromones;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client._MC.Xeno.Structures.PheromoneTowers.UI;

[UsedImplicitly]
public sealed class MCXenoStructurePheromoneTowerBui : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = null!;
    [Dependency] private readonly IInputManager _inputManager = null!;
    [Dependency] private readonly IPlayerManager _player = null!;
    [Dependency] private readonly IEyeManager _eye = null!;

    private readonly SpriteSystem _sprite;
    private readonly TransformSystem _transform;
    private readonly XenoPheromonesSystem _pheromones;

    [ViewVariables] private MCXenoStructuresPheromoneTowerWindow? _window;

    public MCXenoStructurePheromoneTowerBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);

        _sprite = EntMan.System<SpriteSystem>();
        _transform = EntMan.System<TransformSystem>();
        _pheromones = EntMan.System<XenoPheromonesSystem>();
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCXenoStructuresPheromoneTowerWindow>();

        var parent = _window.FindControl<RadialContainer>("Main");

        AddPheromonesButton(XenoPheromones.Frenzy, parent, Owner);
        AddPheromonesButton(XenoPheromones.Warding, parent, Owner);
        AddPheromonesButton(XenoPheromones.Recovery, parent, Owner);

        var vpSize = _displayManager.ScreenSize;
        var pos = _inputManager.MouseScreenPosition.Position / vpSize;

        if (EntMan.TryGetComponent<EyeComponent>(Owner, out var eyeComp) &&
            eyeComp.Target != null)
            pos = _eye.WorldToScreen(_transform.GetMapCoordinates((EntityUid)eyeComp.Target).Position) / vpSize;

        else if (_player.LocalEntity is { } ent)
            pos = _eye.WorldToScreen(_transform.GetMapCoordinates(ent).Position) / vpSize;

        _window.OpenCenteredAt(pos);
    }

    private void AddPheromonesButton(XenoPheromones pheromone, RadialContainer parent, EntityUid owner)
    {
        var name = pheromone.ToString().ToLowerInvariant();
        var suffix = _pheromones.GetPheroSuffix((owner, null));

        if (suffix != null)
            suffix = "_" + suffix;

        var texture = new TextureRect
        {
            VerticalAlignment = Control.VAlignment.Center,
            HorizontalAlignment = Control.HAlignment.Center,
            Texture = _sprite.Frame0(new SpriteSpecifier.Rsi(new ResPath("/Textures/_RMC14/Interface/xeno_pheromones.rsi"), name + suffix)),
            TextureScale = new Vector2(2f, 2f),
        };

        var button = new RadialMenuTextureButton
        {
            StyleClasses = { "RadialMenuButton" },
            SetSize = new Vector2(64, 64),
            ToolTip = name,
        };

        button.OnButtonDown += _ => SendPredictedMessage(new MCXenoStructurePheromoneTowerSelectUIMessage(pheromone));

        button.AddChild(texture);
        parent.AddChild(button);
    }
}
