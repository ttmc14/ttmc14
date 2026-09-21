using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Heal.HealingInfusion.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoHealingInfusionComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId StatusEffectId = "MCStatusEffectXenoHealingInfusion";

    [DataField, AutoNetworkedField]
    public TimeSpan StatusEffectDuration = TimeSpan.FromSeconds(60);

    [DataField, AutoNetworkedField]
    public EntProtoId RayEffectId = "MCEffectTransferMedRay";

    [DataField, AutoNetworkedField]
    public EntProtoId EffectProtoId = "RMCEffectHealHealer";

    [DataField, AutoNetworkedField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_MC/Effects/magic.ogg");
}
