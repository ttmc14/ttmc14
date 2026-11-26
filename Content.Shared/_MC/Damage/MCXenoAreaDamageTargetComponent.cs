using Robust.Shared.GameStates;

namespace Content.Shared._MC.Damage;

/// <summary>
/// Allows xeno area damage to affect this structure.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(MCDamageableSystem))]
public sealed partial class MCXenoAreaDamageTargetComponent : Component;
