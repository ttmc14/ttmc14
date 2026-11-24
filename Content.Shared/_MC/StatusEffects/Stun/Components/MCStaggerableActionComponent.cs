using Robust.Shared.GameStates;

namespace Content.Shared._MC.StatusEffects.Stun.Components;

/// <summary>
/// Prevents the action from being used while its performer is staggered.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MCStaggerableActionComponent : Component;
