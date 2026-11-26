using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._MC.StatusEffects.SlowdownStacks;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class MCSlowdownStacksComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan UpdateNext;

    [DataField, AutoNetworkedField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(0.1f);

    [DataField, AutoNetworkedField]
    public float Stacks;

    /// <summary>Number of stacks recovered per second.</summary>
    [DataField, AutoNetworkedField]
    public float Regeneration = 0.3f;
}
