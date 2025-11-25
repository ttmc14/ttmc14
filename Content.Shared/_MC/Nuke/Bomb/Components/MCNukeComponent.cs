using Robust.Shared.GameStates;

namespace Content.Shared._MC.Nuke.Bomb.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCNukeComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<string> Slots = new()
    {
        "disk_red",
        "disk_blue",
        "disk_green",
    };

    [DataField, AutoNetworkedField]
    public bool Ready;

    [DataField, AutoNetworkedField]
    public bool Safety = true;

    [DataField, AutoNetworkedField]
    public bool Activated;

    [DataField, AutoNetworkedField]
    public TimeSpan Timer;

    [DataField, AutoNetworkedField]
    public TimeSpan Time = TimeSpan.FromSeconds(360);

    [DataField, AutoNetworkedField]
    public TimeSpan TimeMin = TimeSpan.FromSeconds(360);

    [DataField, AutoNetworkedField]
    public TimeSpan TimeMax = TimeSpan.FromSeconds(1200);
}
