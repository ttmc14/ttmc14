using Content.Shared._MC.Xeno.Spit;

// ReSharper disable once CheckNamespace
namespace Content.Client.Weapons.Ranged;

public sealed partial class GunSpreadOverlay
{
    private bool TryGetSpread(EntityUid player, out Angle minSpread, out Angle maxSpread, out Angle currentAngle)
    {
        maxSpread = _guns.GetShooterSpread(player);
        minSpread = maxSpread;
        currentAngle = maxSpread;
        if (!_guns.TryGetGun(player, out _, out var gun))
            return maxSpread != Angle.Zero && _entManager.TryGetComponent<MCXenoSpitComponent>(player, out var spit) && spit.Enabled;

        maxSpread += gun.MaxAngleModified;
        minSpread += gun.MinAngleModified;
        var elapsed = (_timing.CurTime - gun.NextFire).TotalSeconds;
        currentAngle += MathHelper.Clamp(gun.CurrentAngle.Theta - gun.AngleDecayModified.Theta * elapsed,
            gun.MinAngleModified.Theta, gun.MaxAngleModified.Theta);
        return true;
    }
}
