using Content.Shared._MC.Aura;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Heal;

/// <summary>
/// Stores excess healing granted by xenomorph abilities.
/// The reserve absorbs brute and burn damage before health is lost.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoOverhealSystem))]
public sealed partial class MCXenoOverhealComponent : Component
{
    /// <summary>
    /// Remaining brute and burn damage that the current reserve can absorb.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 Amount;

    /// <summary>
    /// Maximum health reserve that can be accumulated through excess healing.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 Maximum = 150;

    /// <summary>
    /// Outline shown on the recipient, scaled by the fraction of the reserve remaining.
    /// </summary>
    [DataField]
    public ProtoId<MCAuraPrototype> Aura = "MCOverhealAura";
}
