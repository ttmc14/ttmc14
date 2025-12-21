using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Wraith.Portal;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPortalComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? LinkedEntity;
}
