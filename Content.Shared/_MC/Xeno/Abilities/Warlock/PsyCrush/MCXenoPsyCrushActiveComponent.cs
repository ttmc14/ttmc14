using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.PsyCrush;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPsyCrushActiveComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityCoordinates TargetCoords;

    [ViewVariables, AutoNetworkedField]
    public int CurrentRadius;

    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextExpansion;

    [ViewVariables, AutoNetworkedField]
    public EntityUid GridUid;

    [ViewVariables, AutoNetworkedField]
    public Vector2i CenterTile;

    [ViewVariables, AutoNetworkedField]
    public HashSet<Vector2i> AffectedTiles = new();

    [ViewVariables, AutoNetworkedField]
    public EntityUid OrbUid;

    [ViewVariables, AutoNetworkedField]
    public HashSet<EntityUid> SpawnedEffects = new();

    [ViewVariables]
    public Queue<Vector2i> Frontier = new();

    [ViewVariables]
    public HashSet<Vector2i> Visited = new();
}
