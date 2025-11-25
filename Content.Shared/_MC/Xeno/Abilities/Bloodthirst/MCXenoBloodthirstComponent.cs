using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Bloodthirst;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoBloodthirstComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan LastFightTime;

    [DataField, AutoNetworkedField]
    public TimeSpan HitZeroTime;

    [DataField, AutoNetworkedField]
    public TimeSpan DecayDelay = TimeSpan.FromSeconds(30);

    [DataField, AutoNetworkedField]
    public TimeSpan DamageDelay = TimeSpan.FromSeconds(30);

    [DataField, AutoNetworkedField]
    public float DecayPerTick = 30f;

    [DataField, AutoNetworkedField]
    public float LowestHealthAllowed = 100;

    [DataField, AutoNetworkedField]
    public float DamagePerDisintegrating = 25;

    [DataField, AutoNetworkedField]
    public bool Disintegrating;

    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_MC/Voice/hiss5.ogg");
}
