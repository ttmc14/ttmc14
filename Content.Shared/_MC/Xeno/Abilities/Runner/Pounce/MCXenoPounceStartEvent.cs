using System.Numerics;
using Robust.Shared.Map;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce;

[ByRefEvent]
public readonly record struct MCXenoPounceStartEvent(EntityUid Uid, MapCoordinates Origin, MapCoordinates Target, Vector2 Direction, float Distance);
