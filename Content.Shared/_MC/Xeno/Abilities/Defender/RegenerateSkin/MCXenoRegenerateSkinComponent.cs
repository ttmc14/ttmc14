using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Defender.RegenerateSkin;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoRegenerateSkinComponent : Component
{
    [DataField, AutoNetworkedField]
    public DamageSpecifier? HealDamage = new()
    {
        DamageDict =
        {
            { "MCBrute", 100 },
            { "MCBurn", 100 },
        },
    };

    [DataField, AutoNetworkedField]
    public int HealSunder = 100;

    [DataField, AutoNetworkedField]
    public ProtoId<EmotePrototype> EffectEmote = "XenoRoar";

    [DataField, AutoNetworkedField]
    public Color EffectColor = Color.FromHex("#b7d5ac");
}
