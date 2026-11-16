using Robust.Shared.GameStates;

namespace Content.Shared._MC.Engineering.Sentries.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCSentryRoundsComponent : Component
{
    [DataField]
    public string ContainerId = "gun_magazine";

    [ViewVariables, AutoNetworkedField]
    public int Rounds;

    [ViewVariables, AutoNetworkedField]
    public int MaxRounds;
}
