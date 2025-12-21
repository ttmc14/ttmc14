using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSlash;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoReagentSlashComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Solution = "chemicals";

    [DataField, AutoNetworkedField]
    public TimeSpan Duration = TimeSpan.FromSeconds(4);

    [DataField, AutoNetworkedField]
    public int Count = 3;

    [DataField, AutoNetworkedField]
    public FixedPoint2 Amount = FixedPoint2.New(7);
}
