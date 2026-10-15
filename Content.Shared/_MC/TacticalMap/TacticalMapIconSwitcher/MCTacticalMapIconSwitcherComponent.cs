using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._MC.TacticalMap.TacticalMapIconSwitcher;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCTacticalMapIconSwitcherComponent : Component
{
    [DataField, AutoNetworkedField]
    public SpriteSpecifier.Rsi? Icon;

    [DataField, AutoNetworkedField]
    public Dictionary<string, SpriteSpecifier.Rsi> Icons = new();
}
