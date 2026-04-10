using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Weapon.StatusEffects;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCProjectileStatusEffectsOnHitComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<MCProjectileStatusEffectEntry> StatusEffects = new();
}

[DataDefinition, Serializable, NetSerializable]
public partial struct MCProjectileStatusEffectEntry
{
    [DataField]
    public EntProtoId EffectId;

    [DataField]
    public TimeSpan? Duration;
}
