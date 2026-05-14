using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Widow.Summon;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoSummonedComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid OwnerUid;

    [ViewVariables, AutoNetworkedField]
    public TimeSpan OutDamageNext;

    [DataField]
    public TimeSpan OutDamageDelay = TimeSpan.FromSeconds(0.5f);

    [DataField]
    public float OutDamageRange = 15;

    [DataField]
    public DamageSpecifier OutDamageSpecifier = new()
    {
        DamageDict =
        {
            { "MCBrute", 25f },
        },
    };
}
