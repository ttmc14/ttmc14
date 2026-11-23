using Content.Shared._MC.Aura;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Drone.ResinJelly.Components;

/// <summary>
/// Configures the fire resistance provided by resin jelly.
/// </summary>
[Access(typeof(MCXenoResinJellySystem))]
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoResinJellyComponent : Component
{
    /// <summary>
    /// Time the protection remains active after use.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan Duration = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Time needed to apply the jelly to another xenomorph.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan DelayOther = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Time needed to apply the jelly to oneself.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan DelaySelf = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Emote played by the recipient when the jelly takes effect.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<EmotePrototype> Emote = "XenoRoar";

    /// <summary>
    /// Visual preset applied while this effect is active.
    /// </summary>
    [DataField]
    public ProtoId<MCAuraPrototype> Aura = "MCResinJellyAura";
}
