namespace Content.Client._MC.Xeno.Hud;

public sealed class MCXenoHudToxinsSystem : OverlayEntitySystem<MCXenoHudToxinsOverlay>
{
    public override MCXenoHudToxinsOverlay Overlay => new();
}
