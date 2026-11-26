using Robust.Shared.GameStates;

// ReSharper disable once CheckNamespace
namespace Content.Shared.Projectiles;

public sealed partial class ProjectileComponent
{
    /// <summary>Damage multiplier captured at launch for this shot.</summary>
    [DataField, AutoNetworkedField]
    public float ShotDamageMultiplier = 1f;
}
