using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Armor.Modules.Features.Binocular.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCModuleBinocularActiveComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public Vector2 Zoom = new(1.25f, 1.25f);

    [ViewVariables, AutoNetworkedField]
    public Vector2 Offset;

    [ViewVariables, AutoNetworkedField]
    public bool CanMove;
}
