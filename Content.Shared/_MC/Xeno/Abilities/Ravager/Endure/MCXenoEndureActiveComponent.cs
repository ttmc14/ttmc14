using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Ravager.Endure;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoEndureActiveComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan EndTime;

    [DataField, AutoNetworkedField]
    public int LastShowedTime;
}
