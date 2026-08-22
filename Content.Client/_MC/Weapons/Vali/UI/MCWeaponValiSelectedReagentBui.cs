using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared._MC.Weapon.Vali.Components;
using Content.Shared._MC.Weapon.Vali.Ui;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._MC.Weapons.Vali.UI;

public sealed partial class MCWeaponValiSelectedReagentBui : BoundUserInterface
{
    private readonly SpriteSystem _sprite;
    private readonly MCUserInterfaceUtilitiesSystem _utilities;

    [ViewVariables] private MCWeaponValiSelectedReagentMenu? _radialMenu;

    public MCWeaponValiSelectedReagentBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);

        _sprite = EntMan.System<SpriteSystem>();
        _utilities = EntMan.System<MCUserInterfaceUtilitiesSystem>();
    }

    protected override void Open()
    {
        base.Open();

        _radialMenu = this.CreateWindow<MCWeaponValiSelectedReagentMenu>();
        var parent = _radialMenu.Main;

        if (EntMan.TryGetComponent<MCWeaponValiComponent>(Owner, out var component))
        {
            AddButton(parent, component.ReagentDefaultIcon, () => SendSelectedReagent());
            foreach (var (reagentId, _) in component.ReagentData)
            {
                var disabled = !component.Reagents.TryGetValue(reagentId, out var reagent) || reagent == FixedPoint2.Zero;
                if (disabled)
                    continue;

                AddButton(parent, component.ReagentData[reagentId].Icon, () => SendSelectedReagent(reagentId));
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

    private void SendSelectedReagent(ProtoId<ReagentPrototype>? reagentId = null)
    {
        SendMessage(new MCWeaponValiSelectReagentMessage(reagentId));
    }
}
