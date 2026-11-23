using Content.Shared._MC.Aura;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Endure;

/// <summary>
/// Configures the temporary armor and survival bonus of Endure.
/// </summary>
[Access(typeof(MCXenoEndureSystem))]
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoEndureComponent : Component
{
    /// <summary>
    /// Time the protection remains active after use.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Emote played when Endure starts.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<EmotePrototype> ActivationEmote = "XenoRoar";

    /// <summary>
    /// Visual preset applied while this effect is active.
    /// </summary>
    [DataField]
    public ProtoId<MCAuraPrototype> Aura = "MCEndureAura";
}
