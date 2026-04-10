using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.StatusEffects.Microwaved;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCMicrowavedComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan TickNext;

    [DataField, AutoNetworkedField]
    public TimeSpan TickDelay = TimeSpan.FromSeconds(0.5);

    [DataField, AutoNetworkedField]
    public int Stacks = 1;

    [DataField, AutoNetworkedField]
    public int MaxStacks = 5;

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            { "MCBurn", 2 },
        },
    };
}
