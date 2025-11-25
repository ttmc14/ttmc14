namespace Content.Server.Explosion.Components;

[RegisterComponent]
public sealed partial class TriggerOnHitComponent : Component
{
    [DataField]
    public bool IgnoreOtherNonHard = true;
}
