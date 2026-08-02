using Content.Shared._RMC14.Marines.Skills;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Engineering.Miners.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCMinerModuleContainerComponent : Component
{
    [DataField, AutoNetworkedField]
    public string ContainerId = "mc_upgrade_module";

    [DataField, AutoNetworkedField]
    public EntityUid? InstalledModule;

    [DataField, AutoNetworkedField]
    public EntProtoId<SkillDefinitionComponent> SkillId = "MCSkillEngineer";

    [DataField, AutoNetworkedField]
    public int SkillLevel = 3;
}
