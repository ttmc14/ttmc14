namespace Content.Client._MC.Xeno.Abilities.Heal;

public sealed class MCXenoBeamSystem : OverlayEntitySystem<MCXenoBeamOverlay>
{
    public override MCXenoBeamOverlay Overlay => new();
}
