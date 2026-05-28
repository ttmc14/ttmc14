using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.ZLevels.Weapons.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCZLevelProjectileVisualOffsetComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2 Offset;
    public Vector2? OriginalOffset;
    public Vector2 AppliedOffset;
}
