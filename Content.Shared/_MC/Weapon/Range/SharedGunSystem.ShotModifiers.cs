using Content.Shared._MC.Weapon.Range.Events;
using Content.Shared._RMC14.Random;
using Robust.Shared.Map;

// ReSharper disable once CheckNamespace
namespace Content.Shared.Weapons.Ranged.Systems;

public abstract partial class SharedGunSystem
{
    [Dependency] private readonly RMCPseudoRandomSystem _shotRandom = null!;

    /// <summary>Returns the additional spread contributed by the shooter.</summary>
    public Angle GetShooterSpread(EntityUid? shooter)
    {
        var ev = new GetShooterSpreadEvent(Angle.Zero);
        if (shooter is { } uid)
        {
            RaiseLocalEvent(uid, ref ev);
        }

        return ev.AdditionalSpread;
    }

    /// <summary>Returns the damage multiplier contributed by the shooter.</summary>
    public float GetShooterDamageMultiplier(EntityUid? shooter)
    {
        var ev = new GetShooterDamageMultiplierEvent(1f);
        if (shooter is { } uid)
        {
            RaiseLocalEvent(uid, ref ev);
        }

        return ev.Multiplier;
    }

    private EntityCoordinates ApplyShooterSpread(EntityUid? user, EntityUid gun, MapCoordinates origin, EntityCoordinates target)
    {
        var spread = GetShooterSpread(user);
        if (spread == Angle.Zero)
            return target;

        var offset = _shotRandom.NextAngle(gun, -spread / 2, spread / 2);
        var direction = target.ToMapPos(EntityManager, TransformSystem) - origin.Position;
        var adjusted = new MapCoordinates(origin.Position + offset.RotateVec(direction), origin.MapId);
        return TransformSystem.ToCoordinates(target.EntityId, adjusted);
    }
}
