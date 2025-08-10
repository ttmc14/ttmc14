using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.RegenerateSkin;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoRegenerateSkinComponent : Component
{
    [DataField, AutoNetworkedField]
    public DamageSpecifier Heal = new();

    [DataField, AutoNetworkedField]
    public int Sunder = 100;

    [DataField, AutoNetworkedField]
    public ProtoId<EmotePrototype> Emote = "XenoRoar";
}
