using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Weapon.Aimed;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCAimedShootComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ActionId = "MCActionToggleAimedShoot";

    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    [DataField, AutoNetworkedField]
    public float AimFireModifier = 1;

    [DataField, AutoNetworkedField]
    public float AimSpeedModifier = 1;

    [DataField, AutoNetworkedField]
    public bool Active;
}
