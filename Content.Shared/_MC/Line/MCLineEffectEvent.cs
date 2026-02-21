using Robust.Shared.Serialization;

namespace Content.Shared._MC.Line;

[Serializable, NetSerializable]
public sealed class MCLineEffectEvent : EntityEventArgs
{
    public List<MCLineSpriteData> Data = new();
}
