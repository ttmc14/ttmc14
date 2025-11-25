using Robust.Shared.GameStates;

namespace Content.Shared._MC.Nuke.Generator.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCNukeDiskGeneratorRunningComponent : Component
{
    [AutoNetworkedField]
    public TimeSpan StartTime;
}
