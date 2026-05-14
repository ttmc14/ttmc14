using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Panther.EvasiveManeuvers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoEvasiveManeuversComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Active;

    [DataField, AutoNetworkedField]
    public TimeSpan LastMove;

    [DataField, AutoNetworkedField]
    public TimeSpan MoveTolerance = TimeSpan.FromSeconds(0.5f);

    /// <remarks>
    /// Plasma/s
    /// </remarks>
    [DataField]
    public float PlasmaDrain = 30f;

    [DataField]
    public float PlasmaInterruptDrain = 65f;

    #region Update

    [DataField, AutoNetworkedField]
    public TimeSpan UpdateNext;

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(0.5f);

    #endregion

    #region Effects

    [DataField, AutoNetworkedField]
    public SoundSpecifier? EffectSoundEvasion = new SoundCollectionSpecifier("XenoTailSwipe");

    #endregion
}
