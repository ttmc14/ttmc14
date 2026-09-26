using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Weapons.Range.Aimed;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCAimedShootComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ActionId = "MCActionToggleAimedShoot";

    [DataField, AutoNetworkedField]
    public float AimFireModifier = 1;

    [DataField, AutoNetworkedField]
    public float AimSpeedModifier = 1;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public EntityUid? Action;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public bool Active;
}
