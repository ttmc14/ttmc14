using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._MC.Xeno.Abilities.Wraith.Banish;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoBanishedComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid User;

    [ViewVariables, AutoNetworkedField]
    public MapCoordinates Position;

    [ViewVariables, AutoNetworkedField]
    public TimeSpan EndTime;
}
