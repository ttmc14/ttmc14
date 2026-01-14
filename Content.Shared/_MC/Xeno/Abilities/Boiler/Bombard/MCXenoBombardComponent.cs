using Content.Shared.DoAfter;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.Bombard;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoBombardComponent : Component
{
    [DataField, AutoNetworkedField]
    public int? MinDistance = 5;

    [DataField, AutoNetworkedField]
    public float AmmoCooldownReduction = 1.5f;

    [DataField, AutoNetworkedField]
    public float ProjectileSpeed = 15f;

    [DataField, AutoNetworkedField]
    public TimeSpan DiggingDuration = TimeSpan.FromSeconds(3);

    [DataField, AutoNetworkedField]
    public TimeSpan LaunchDuration = TimeSpan.FromSeconds(2);

    [ViewVariables, AutoNetworkedField]
    public bool Digging;

    [ViewVariables]
    public DoAfterId? DiggingDoAfterId;
}
