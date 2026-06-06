using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Armor.Modules.Features.Shield.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCModuleShieldComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Health;

    [DataField, AutoNetworkedField]
    public float HealthMax = 40;

    [DataField, AutoNetworkedField]
    public float RechargeAmount = 10f;

    [DataField, AutoNetworkedField]
    public TimeSpan RechargeRate = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public TimeSpan RechargeCooldown = TimeSpan.FromSeconds(15);

    [DataField, AutoNetworkedField]
    public bool Recharging;

    [DataField, AutoNetworkedField]
    public Color ShieldColorLow = Color.Maroon;

    [DataField, AutoNetworkedField]
    public Color ShieldColorMid = new(200, 0, 0);

    [DataField, AutoNetworkedField]
    public Color ShieldColorFull = Color.LightBlue;

    [DataField, AutoNetworkedField]
    public Color? CurrentColor;

    [ViewVariables]
    public TimeSpan RechargeCooldownEndTime;

    [ViewVariables]
    public TimeSpan NextRechargeTick;

    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundRecharge = new SoundPathSpecifier("/Audio/_MC/Effects/eshield_recharge.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundDown = new SoundPathSpecifier("/Audio/_MC/Effects/eshield_down.ogg");
}
