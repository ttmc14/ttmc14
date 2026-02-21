using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Line;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCLineComponent : Component
{
    [DataField]
    public SpriteSpecifier? Head;

    [DataField]
    public SpriteSpecifier? Body;

    [DataField]
    public SpriteSpecifier? Tail;
}
