using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.StatusEffects.Sleep;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCEffectSleepComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public TimeSpan TickNext;

    [DataField, AutoNetworkedField]
    public TimeSpan TickDelay = TimeSpan.FromSeconds(0.5);

    [DataField]
    public float HealingDamage = -0.0125f;

    [DataField]
    public DamageSpecifier HealingDamageModifier = new()
    {
        DamageDict =
        {
            { "MCBrute", 1f },
            { "MCBurn",  1f },
            { "MCToxin", 0.5f },
            { "MCClone", 0.8f},
        },
    };

    [DataField]
    public Dictionary<ProtoId<DamageTypePrototype>, bool> HealingDamageRationUsage = new()
    {
        { "MCClone", true },
    };
}
