using Content.Shared._MC.Aura;
using Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Systems.Effects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Components;

/// <summary>
/// Configures the outline that grows with the Ravager's rage.
/// </summary>
[Access(typeof(MCXenoRageAuraSystem))]
[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoRageAuraComponent : Component
{
    /// <summary>
    /// Visual preset applied while this effect is active.
    /// </summary>
    [DataField]
    public ProtoId<MCAuraPrototype> Aura = "MCRageAura";
}
