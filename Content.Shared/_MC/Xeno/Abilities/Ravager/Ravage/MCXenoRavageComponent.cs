using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Ravage;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoRavageComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<EmotePrototype> Emote = "XenoRoar";
}
