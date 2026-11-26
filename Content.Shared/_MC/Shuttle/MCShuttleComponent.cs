using Content.Shared._MC.Shuttle.Console.UI;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Shuttle;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCShuttleComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Evacuated;

    [DataField]
    public MCEvacuationPhase Phase;

    [DataField]
    public TimeSpan NextTransition;
}
