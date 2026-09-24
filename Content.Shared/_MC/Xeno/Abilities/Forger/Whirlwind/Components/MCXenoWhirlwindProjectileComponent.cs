using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
[Access(typeof(MCXenoWhirlwindProjectileSystem))]
public sealed partial class MCXenoWhirlwindProjectileComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId SpawnId = "MCTileFireXenoAcid";

    [DataField, AutoNetworkedField]
    public float SpawnRange = 1;

    [DataField, AutoNetworkedField]
    public float MexRange = 7;

    [DataField, AutoNetworkedField]
    public DamageSpecifier HitDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "MCBurn", 50 },
        },
    };
}
