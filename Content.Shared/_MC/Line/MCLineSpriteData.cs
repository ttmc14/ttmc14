using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Line;

[Serializable, NetSerializable]
public readonly record struct MCLineSpriteData(NetCoordinates Coordinates, Angle Angle, SpriteSpecifier Sprite, float Scale, EntProtoId ProtoId);
