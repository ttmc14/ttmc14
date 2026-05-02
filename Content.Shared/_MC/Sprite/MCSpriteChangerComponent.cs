using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Sprite;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCSpriteChangerComponent : Component
{
    [DataField, AutoNetworkedField]
    public ResPath Path = ResPath.Empty;
}
