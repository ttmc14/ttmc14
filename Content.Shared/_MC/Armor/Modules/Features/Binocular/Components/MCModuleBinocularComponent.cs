using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Armor.Modules.Features.Binocular.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCModuleBinocularComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ActionId = "MCActionModuleToggleBinocular";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionUid;

    [DataField, AutoNetworkedField]
    public Vector2 Zoom = new(1.75f, 1.75f);

    [DataField, AutoNetworkedField]
    public int OffsetLength = 11;

    [DataField, AutoNetworkedField]
    public bool CanMove;
}
