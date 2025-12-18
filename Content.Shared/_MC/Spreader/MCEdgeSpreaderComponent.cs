using Robust.Shared.GameStates;

namespace Content.Shared._MC.Spreader;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCEdgeSpreaderComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.1f);

    [DataField, AutoNetworkedField]
    public TimeSpan NextUpdate;

    [DataField, AutoNetworkedField]
    public int Range = 5;

    [AutoNetworkedField]
    public EntityUid CreatorUid;
}
