using Robust.Shared.GameStates;

namespace Content.Shared._MC.ZLevels.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCZLevelFallStunModifierComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Modifier = 1f;
}
