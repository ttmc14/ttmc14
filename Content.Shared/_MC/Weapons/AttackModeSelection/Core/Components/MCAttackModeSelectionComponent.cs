using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Weapons.AttackModeSelection.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCAttackModeSelectionComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ActionId = "MCActionSelectFireMode";

    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    [DataField]
    public Dictionary<string, MCAttackModeSelectionEntry> Modes = new();
}

[DataDefinition, Serializable]
public partial struct MCAttackModeSelectionEntry
{
    [DataField]
    public SpriteSpecifier.Rsi Icon;
}
