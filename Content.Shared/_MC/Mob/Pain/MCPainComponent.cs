using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Mob.Pain;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCPainComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Painloss;

    [DataField, AutoNetworkedField]
    public float MovementSpeedModifier = 1f;

    [DataField, AutoNetworkedField]
    public Dictionary<float, float> MovementSpeedModifiers = new()
    {
        { 100, 0.7f },
        { 125, 0.5f },
        { 150, 0.3f },
    };

    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<DamageTypePrototype>, float> DamageLosseModifiers = new()
    {
        { "MCOxygen", 0.75f },
        { "MCToxin",  0.75f },
        { "MCBurn",   1.25f },
        { "MCBrute",  1.00f },
        { "MCClone",  1.00f },
    };
}
