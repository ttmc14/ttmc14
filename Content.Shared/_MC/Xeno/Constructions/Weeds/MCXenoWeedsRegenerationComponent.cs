using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Constructions.Weeds;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoWeedsRegenerationComponent : Component
{
    [DataField]
    public float HealthModifier = 1.2f;

    [DataField]
    public float PlasmaModifier = 1.2f;

    [DataField]
    public float SunderModifier = 2f;
}
