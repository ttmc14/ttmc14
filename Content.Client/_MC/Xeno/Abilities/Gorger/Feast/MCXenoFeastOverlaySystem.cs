namespace Content.Client._MC.Xeno.Abilities.Gorger.Feast;

public sealed class MCXenoFeastOverlaySystem : OverlayEntitySystem<MCXenoFeastOverlay>
{
    public override MCXenoFeastOverlay Overlay => new();
}
