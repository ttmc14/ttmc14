using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Xeno.Abilities.Wraith.PortalPlacer;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPortalPlacerComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2 Offset;

    [DataField, AutoNetworkedField]
    public EntityUid? PortalFirstEntityUid;

    [DataField, AutoNetworkedField]
    public EntityUid? PortalSecondEntityUid;

    [DataField, AutoNetworkedField]
    public PortalEntry PortalFirst = new();

    [DataField, AutoNetworkedField]
    public PortalEntry PortalSecond = new();

    [DataDefinition, Serializable, NetSerializable]
    public sealed partial class PortalEntry
    {
        [DataField]
        public EntProtoId Id;

        [DataField]
        public SpriteSpecifier.Rsi Icon;
    }
}

[Serializable, NetSerializable]
public enum MCXenoPortalPlacerUI
{
    Key,
}

[Serializable, NetSerializable]
public sealed class MCXenoChoosePortalBuiMsg : BoundUserInterfaceMessage
{
    public readonly EntProtoId Id;

    public MCXenoChoosePortalBuiMsg(EntProtoId id)
    {
        Id = id;
    }
}
