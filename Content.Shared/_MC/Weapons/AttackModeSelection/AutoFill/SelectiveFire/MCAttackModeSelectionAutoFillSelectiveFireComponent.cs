using Robust.Shared.GameStates;
using Robust.Shared.Utility;

using SelectiveFireType = Content.Shared.Weapons.Ranged.Components.SelectiveFire;

namespace Content.Shared._MC.Weapons.AttackModeSelection.AutoFill.SelectiveFire;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCAttackModeSelectionAutoFillSelectiveFireComponent : Component
{
    [DataField]
    public Dictionary<SelectiveFireType, SpriteSpecifier.Rsi> Icons = new();
}
