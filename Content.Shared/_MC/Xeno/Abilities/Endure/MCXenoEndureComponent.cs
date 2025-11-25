using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Endure;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoEndureComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);

    [DataField, AutoNetworkedField]
    public ProtoId<EmotePrototype> ActivationEmote = "XenoRoar";

    [DataField, AutoNetworkedField]
    public Color ActivationAuraColor = Color.FromHex("#800080");
}
