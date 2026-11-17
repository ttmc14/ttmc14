using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.StatusEffects.Microwaved;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCMicrowavedComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public TimeSpan TickNext;

    [DataField]
    public TimeSpan TickDelay = TimeSpan.FromSeconds(0.5);

    [DataField]
    public int Stacks = 1;

    [DataField]
    public int MaxStacks = 7;

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            { "MCBurn", 2 },
        },
    };
}
