using Robust.Shared.GameStates;

namespace Content.Shared._MC.Rules.Common;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCRuleXenoMapComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid? Map;
}
