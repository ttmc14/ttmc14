using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Evolution;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoEvolutionRequiredQuantityComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Count = 8;
}
