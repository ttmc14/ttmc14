using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Rules.Utilities;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCRuleSpawnNukeDiskGeneratorsComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public bool Spawned;

    [DataField]
    public List<EntProtoId> EntityIds = new()
    {
        "MCComputerNukeDiskGeneratorRed",
        "MCComputerNukeDiskGeneratorGreen",
        "MCComputerNukeDiskGeneratorBlue",
    };
}
