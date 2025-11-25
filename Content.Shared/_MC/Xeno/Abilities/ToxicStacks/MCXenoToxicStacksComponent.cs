using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.ToxicStacks;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoToxicStacksComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextTick;

    [ViewVariables, AutoNetworkedField]
    public int Count;

    [DataField, AutoNetworkedField]
    public int Max = 30;

    [DataField, AutoNetworkedField]
    public DamageSpecifier BaseDamage = new()
    {
        DamageDict =
        {
            { "MCBurn", 1 }
        },
    };

    [DataField, AutoNetworkedField]
    public DamageSpecifier StacksDamage = new()
    {
        DamageDict =
        {
            { "MCBurn", 1 }
        },
    };

    [DataField, AutoNetworkedField]
    public TimeSpan TickInterval = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public int Decay = 1;
}

[Serializable, NetSerializable]
public enum MCXenoToxicStacksLayer
{
    Base,
    Icon,
}

[Serializable, NetSerializable]
public enum MCXenoToxicStacksVisuals
{
    Visuals,
}
