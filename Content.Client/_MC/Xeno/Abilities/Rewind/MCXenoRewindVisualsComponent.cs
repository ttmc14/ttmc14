using Robust.Shared.Map;

namespace Content.Client._MC.Xeno.Abilities.Rewind;

[RegisterComponent]
public sealed partial class MCXenoRewindVisualsComponent : Component
{
    public readonly List<(EntityCoordinates Position, Angle Rotation)> Path = new();
    public int ImageCount;
    public int NextImage;
    public TimeSpan PlaybackStart;
}
