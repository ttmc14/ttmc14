using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Wraith.Blink;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoBlinkComponent : Component
{
    [DataField]
    public float DragMultiplier = 20;

    [DataField]
    public TimeSpan DragDelay = TimeSpan.FromSeconds(0.5f);

    [DataField]
    public float DragFriendlyMultiplier = 4;

    [DataField]
    public float Range = 3;

    [DataField]
    public float EffectRange = 1f;

    [DataField]
    public TimeSpan StaggerDuration = TimeSpan.FromSeconds(2);

    [DataField]
    public float SlowdownStacks = 3;
}
