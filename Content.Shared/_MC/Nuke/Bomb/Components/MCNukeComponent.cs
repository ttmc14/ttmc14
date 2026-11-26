using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._MC.Nuke.Bomb.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MCNukeComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<string> Slots = new() { "disk_red", "disk_green", "disk_blue" };

    [DataField, AutoNetworkedField]
    public bool Deployed;

    [DataField, AutoNetworkedField]
    public bool Safety = true;

    [DataField, AutoNetworkedField]
    public bool Disabled;

    [DataField, AutoNetworkedField]
    public bool Exploded;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan? DetonatesAt;

    [DataField, AutoNetworkedField]
    public TimeSpan Time = TimeSpan.FromMinutes(6);

    [DataField, AutoNetworkedField]
    public TimeSpan TimeMin = TimeSpan.FromMinutes(6);

    [DataField, AutoNetworkedField]
    public TimeSpan TimeMax = TimeSpan.FromMinutes(20);

    [DataField]
    public SoundSpecifier ActivationMusic = new SoundCollectionSpecifier("RMCHijack", AudioParams.Default.WithVolume(-8));

    [DataField]
    public EntityUid? MusicStream;
}

[Serializable, NetSerializable]
public enum MCNukeVisuals
{
    State,
}
