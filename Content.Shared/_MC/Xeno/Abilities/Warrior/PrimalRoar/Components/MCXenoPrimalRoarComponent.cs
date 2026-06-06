using Content.Shared._MC.CameraShake;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Warrior.PrimalRoar.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoPrimalRoarComponent : Component
{
    [DataField]
    public int StacksCost = 8;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(8);

    [DataField]
    public float Range = 6.5f;

    [DataField]
    public MCCameraShakeEntry DebuffShake = new(6, 3);

    [DataField]
    public float AllyDamageBuff = 1.18f;

    [DataField]
    public float AllySpeedBuff = 1.12f;
}
