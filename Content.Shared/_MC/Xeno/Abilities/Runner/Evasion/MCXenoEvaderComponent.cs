using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Evasion;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoEvaderComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public TimeSpan EndTime;

    [ViewVariables, AutoNetworkedField]
    public float Stacks;
}
