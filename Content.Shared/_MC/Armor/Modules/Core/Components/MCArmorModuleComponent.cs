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
    public string? VisualsLayer;

    [DataField, AutoNetworkedField]
    public MCArmorDefinition Armor;

    [DataField]
    public TimeSpan DurationEquip = TimeSpan.FromSeconds(3.5);

    [DataField]
    public TimeSpan DurationUnequip = TimeSpan.FromSeconds(3.5);

    [DataField]
    public ComponentRegistry Components = new();
}
