using Robust.Shared.GameStates;

namespace Content.Shared._MC.Rules.Utilities;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCRuleRecalculatePowerComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public bool Recalculated;
}
