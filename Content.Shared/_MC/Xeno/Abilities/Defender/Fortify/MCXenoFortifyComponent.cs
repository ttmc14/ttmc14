using Content.Shared._RMC14.Stun;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Collision.Shapes;

namespace Content.Shared._MC.Xeno.Abilities.Defender.Fortify;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoFortifyComponent : Component
{
    public const string FixtureId = "cm-xeno-fortify";

    [DataField, AutoNetworkedField]
    public bool Fortified;

    [ViewVariables, AutoNetworkedField]
    public int ArmorFlat = 50;

    [DataField, AutoNetworkedField]
    public string[] ImmuneToStatuses = { "KnockedDown" };

    [DataField]
    public IPhysShape Shape = new PhysShapeCircle(0.49f);

    [DataField, AutoNetworkedField]
    public RMCSizes FortifySize = RMCSizes.Immobile;

    [DataField, AutoNetworkedField]
    public RMCSizes? OriginalSize;

    [DataField, AutoNetworkedField]
    public bool BaseWeakToExplosionStuns = true;

    [DataField, AutoNetworkedField]
    public bool CanMoveFortified;

    [DataField, AutoNetworkedField]
    public FixedPoint2 MoveSpeedModifier = FixedPoint2.New(0.45);

    [DataField, AutoNetworkedField]
    public SoundSpecifier FortifySound = new SoundPathSpecifier("/Audio/Effects/stonedoor_openclose.ogg", AudioParams.Default.WithVariation(0.2f));
}
