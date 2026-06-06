using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Armor.Modules.Features.HealthScan.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class  MCModuleHealthScanComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ActionId = "MCActionModuleMedicalScan";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionUid;
}
