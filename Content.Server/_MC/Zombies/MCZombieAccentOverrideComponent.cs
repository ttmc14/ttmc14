namespace Content.Server._MC.Zombies;

/// <summary>
/// Overrides the applied accent for zombies.
/// </summary>
[RegisterComponent]
public sealed partial class MCZombieAccentOverrideComponent : Component
{
    [DataField("accent")]
    public string Accent = "zombie";
}
