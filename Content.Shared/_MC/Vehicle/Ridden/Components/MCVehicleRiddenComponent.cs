using System.Numerics;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Vehicle.Ridden.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCVehicleRiddenComponent : Component
{
    [AutoNetworkedField]
    public bool Operated;

    [AutoNetworkedField]
    public Vector2 LastPosition;

    [DataField, AutoNetworkedField]
    public float Fuel = 1000f;

    [DataField]
    public float FuelCost = 1f;

    [DataField]
    public float FuelMax = 1000f;

    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, float> AllowedReagents = new()
    {
        { "WeldingFuel", 1 },
    };

    [DataField]
    public EntityWhitelist? RefillWhitelist;

    [DataField]
    public int HandsRequired = 1;
}
