using Content.Shared._MC.Armor;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Sentinel.DrainSting;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
[Access(typeof(MCXenoDrainStingSystem), Other = AccessPermissions.None)]
public sealed partial class MCXenoDrainStingBuffComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan EndTime = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public MCArmorDefinition Armor = new()
    {
        Acid = 20,
        Bio = 20,
        Bomb = 20,
        Bullet = 30,
        Energy = 30,
        Fire = 20,
        Laser = 30,
        Melee = 20,
    };
}
