using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Panther.Adrenalin;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoAdrenalinComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan LastStep;

    [DataField, AutoNetworkedField]
    public TimeSpan UpdateNext;

    [DataField]
    public TimeSpan GainDelay = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    [DataField]
    public float GainPlasma = 2f;

    [DataField]
    public float DrainPlasmaMin = 40f;

    [DataField]
    public float DrainPlasma = 3f;
}
