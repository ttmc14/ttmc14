using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
[Access(typeof(MCXenoWhirlwindProjectileSystem))]
public sealed partial class MCXenoWhirlwindProjectileComponent : Component
{
    /// <summary>
    /// Fire spawned on tiles crossed by the whirlwind and on adjacent tiles.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId SpawnId = "MCTileFireXenoAcid";

    /// <summary>
    /// Flight speed in meters per second.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Speed = 5;

    /// <summary>
    /// Maximum distance the whirlwind travels from its launch position, in meters.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxRange = 8;

    /// <summary>
    /// Radius in meters within which the whirlwind burns a mob.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ContactRadius = 0.45f;

    /// <summary>
    /// Damage dealt on contact with a mob or on impact with a damageable obstacle.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier HitDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "MCBurn", 50 },
        },
    };

    /// <summary>
    /// Intensity of the fire applied to a mob on contact.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int FireIntensity = 30;

    /// <summary>
    /// Burn duration passed to the flammability system on contact.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int FireDuration = 20;

    /// <summary>
    /// Fire stacks added to a mob on each new contact.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FireStacks = 2;

    /// <summary>
    /// End of the flight path in the launch parent's coordinate space.
    /// </summary>
    [ViewVariables]
    public EntityCoordinates Destination;

    /// <summary>
    /// Mobs currently touching the whirlwind. A mob can be hit again after leaving the radius.
    /// </summary>
    [ViewVariables]
    public readonly HashSet<Entity<MobStateComponent>> Contacts = new();
}
