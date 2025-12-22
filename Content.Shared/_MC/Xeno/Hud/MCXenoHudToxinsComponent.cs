using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Hud;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoHudToxinsComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Solution = "chemicals";

    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<ReagentPrototype>, string> Reagents = new()
    {
        { "MCHemodile", "hemodile" },
        { "MCNeurotoxin", "neurotoxin" },
        { "MCOzelomelyn", "ozelomelyn" },
        { "MCTransvitox", "transvitox" },
        { "MCSanguinal", "sanguinal" },
    };

    [DataField, AutoNetworkedField]
    public string ReagentHighPostfix = "_high";

    [DataField, AutoNetworkedField]
    public FixedPoint2 ReagentHighQuantity = FixedPoint2.New(30);
}
