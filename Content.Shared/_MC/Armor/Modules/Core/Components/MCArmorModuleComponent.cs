using Content.Shared._MC.Armor.Core.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Armor.Modules.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCArmorModuleComponent : Component
{
    [DataField, AutoNetworkedField]
    public SpriteSpecifier.Rsi? Visuals;

    [DataField, AutoNetworkedField]
    public MCArmorDefinition Armor;

    [DataField]
    public ComponentRegistry Components = new();

    [DataField]
    public ComponentRegistry UserComponents = new();
}
