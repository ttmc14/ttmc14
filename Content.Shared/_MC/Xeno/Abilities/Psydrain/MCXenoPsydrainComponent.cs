using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Psydrain;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoPsydrainSystem))]
public sealed partial class MCXenoPsydrainComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(5);

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            { "Cellular", 20 },
        },
    };

    #region Rewards

    [DataField, AutoNetworkedField]
    public int PsypointRewardMin = 30;

    [DataField, AutoNetworkedField]
    public int PsypointRewardMax = 90;

    [DataField, AutoNetworkedField]
    public int BiomassGain = 15;

    [DataField, AutoNetworkedField]
    public int LarvaPointsGain = 1;

    #endregion

    #region Jittering

    [DataField, AutoNetworkedField]
    public TimeSpan JitteringDelayOwner = TimeSpan.FromSeconds(4);

    [DataField, AutoNetworkedField]
    public float JitteringFrequencyOwner = 2f;

    [DataField, AutoNetworkedField]
    public float JitteringAmplitudeOwner = 10f;

    [DataField, AutoNetworkedField]
    public TimeSpan JitteringDelayTarget = TimeSpan.FromSeconds(7);

    [DataField, AutoNetworkedField]
    public float JitteringAmplitudeTarget = 10f;

    [DataField, AutoNetworkedField]
    public float JitteringFrequencyTarget = 4f;

    #endregion

    #region Sounds

    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundDrain = new SoundPathSpecifier("/Audio/_MC/Effects/nightfall.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundDrainEnd = new SoundPathSpecifier("/Audio/_MC/Effects/end_of_psy_drain.ogg");

    #endregion
}

