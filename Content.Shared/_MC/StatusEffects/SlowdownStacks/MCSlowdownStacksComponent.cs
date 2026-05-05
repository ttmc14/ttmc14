using Robust.Shared.GameStates;

namespace Content.Shared._MC.StatusEffects.SlowdownStacks;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCSlowdownStacksComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan UpdateNext;

    [DataField, AutoNetworkedField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(0.5f);

    [DataField, AutoNetworkedField]
    public float Stacks;

    [DataField, AutoNetworkedField]
    public float Regeneration = 0.3f;
}
