using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared._MC.Weapons.AttackModeSelection;
using Content.Shared._MC.Weapons.AttackModeSelection.Core.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client._MC.Weapons.AttackModeSelection.UI;

public sealed partial class MCWeaponAttackModeSelectionBui : BoundUserInterface
{
    private readonly SpriteSystem _sprite;
    private readonly MCUserInterfaceUtilitiesSystem _utilities;

    [ViewVariables] private MCWeaponAttackModeSelectionMenu? _radialMenu;

    public MCWeaponAttackModeSelectionBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);

        _sprite = EntMan.System<SpriteSystem>();
        _utilities = EntMan.System<MCUserInterfaceUtilitiesSystem>();
    }

    protected override void Open()
    {
        base.Open();

        _radialMenu = this.CreateWindow<MCWeaponAttackModeSelectionMenu>();

        var parent = _radialMenu.Main;
        if (EntMan.TryGetComponent<MCAttackModeSelectionComponent>(Owner, out var component))
        {
            foreach (var (id, mode) in component.Modes)
            {
                AddButton(parent, mode.Icon, () => Select(id));
            }
        }

        _radialMenu.OpenCenteredAt(_utilities.GetRadialPosition(Owner));
    }

    private void AddButton(RadialContainer parent, SpriteSpecifier.Rsi icon, Action onButtonDown)
    {
        var texture = new TextureRect
        {
            VerticalAlignment = Control.VAlignment.Center,
            HorizontalAlignment = Control.HAlignment.Center,
            Texture = _sprite.Frame0(icon),
            TextureScale = new Vector2(2f, 2f),
        };

        var button = new RadialMenuTextureButton
        {
            StyleClasses = { "RadialMenuButton" },
            SetSize = new Vector2(64, 64),
        };

        button.OnButtonDown += _ => onButtonDown.Invoke();

        button.AddChild(texture);
        parent.AddChild(button);
    }

    private void Select(string id)
    {
        SendMessage(new MCAttackModeSelectionMessage(id));
    }
}
