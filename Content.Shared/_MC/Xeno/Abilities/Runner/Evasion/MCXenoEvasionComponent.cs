using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Evasion;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoEvasionComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Duration = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public float RefreshThreshold = 120;

    // TODO: Custom sound collection
    [DataField, AutoNetworkedField]
    public SoundSpecifier? EvadeSound = new SoundCollectionSpecifier("XenoTailSwipe");
}

[Serializable, NetSerializable]
public enum EvasionLayer
{
    Base,
}

[Serializable, NetSerializable]
public enum EvasionVisuals
{
    Visuals,
}
