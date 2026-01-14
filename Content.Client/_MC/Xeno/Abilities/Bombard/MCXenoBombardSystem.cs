namespace Content.Client._MC.Xeno.Abilities.Bombard;

public sealed class MCXenoBombardSystem : OverlayEntitySystem<MCXenoBombardOverlay>
{
    public override MCXenoBombardOverlay Overlay => new();
}
