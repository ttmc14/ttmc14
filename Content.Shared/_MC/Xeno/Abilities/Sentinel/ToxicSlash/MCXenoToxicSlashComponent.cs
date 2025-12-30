using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Sentinel.ToxicSlash;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoToxicSlashComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Duration = TimeSpan.FromSeconds(4);

    [DataField, AutoNetworkedField]
    public int Stacks = 5;

    [DataField, AutoNetworkedField]
    public int Slashes = 3;
}
