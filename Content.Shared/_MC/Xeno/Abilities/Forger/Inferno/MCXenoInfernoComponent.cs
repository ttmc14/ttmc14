using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Inferno;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoInfernoComponent : Component
{
    [DataField, AutoNetworkedField]
    public float PositionInfernoX = 2;

    [DataField, AutoNetworkedField]
    public float PositionInfernoY = 2;

    [DataField, AutoNetworkedField]
    public TimeSpan InfernoDelay = TimeSpan.FromSeconds(0.5);

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new();

    [DataField, AutoNetworkedField]
    public float Range = 2.5f;

    [DataField, AutoNetworkedField]
    public EntProtoId Effect = "MCEffectInfernoPyrogen";

    [DataField, AutoNetworkedField]
    public EntProtoId Spawn = "MCTileFireXenoAcid";

    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_MC/Effects/Pyrogen/fireball.ogg");
}
