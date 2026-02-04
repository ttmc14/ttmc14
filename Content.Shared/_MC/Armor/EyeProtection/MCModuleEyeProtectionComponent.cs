using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Armor.EyeProtection;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCModuleEyeProtectionComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ActionId = "MCActionModuleToggleWelding";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionUid;

    [ViewVariables, AutoNetworkedField]
    public bool Enabled;
}
