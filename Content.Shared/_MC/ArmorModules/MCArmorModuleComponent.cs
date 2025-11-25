using Content.Shared._MC.Armor;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._MC.ArmorModules;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCArmorModuleComponent : Component
{
    [DataField, AutoNetworkedField]
    public SpriteSpecifier.Rsi? Sprite;

    [DataField, AutoNetworkedField]
    public MCArmorDefinition Armor;

    [DataField]
    public ComponentRegistry Components = new();
}
