using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.DrainSting;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoDrainStingComponent : Component
{
    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            { "MCBurn", 1 },
        },
    };
}
