using Content.Shared._RMC14.CameraShake;

namespace Content.Shared._MC.CameraShake;

public sealed class MCCameraShakeSystem : EntitySystem
{
    [Dependency] private readonly RMCCameraShakeSystem _rmcCameraShake = null!;

    public void ShakeCamera(EntityUid user, MCCameraShakeEntry entry)
    {
        _rmcCameraShake.ShakeCamera(user, entry.Shakes, entry.Strength, entry.Spacing);
    }

    public void ShakeCamera(EntityUid user, int shakes, int strength, TimeSpan? spacing = null)
    {
        _rmcCameraShake.ShakeCamera(user, shakes, strength, spacing);
    }
}
