using Robust.Shared.GameStates;

namespace Content.Shared._MC.StatusEffects.Shatter;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCShatterComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Modifier = 0.2f;
}
