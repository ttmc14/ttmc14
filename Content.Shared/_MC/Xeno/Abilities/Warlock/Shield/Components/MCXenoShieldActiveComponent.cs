using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.Shield.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoShieldActiveComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid? ShieldUid;

    [ViewVariables, AutoNetworkedField]
    public Angle LocalRotation;
}
