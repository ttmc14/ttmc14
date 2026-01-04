using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSlash;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoReagentSlashActiveComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan ExpiresTime;

    [DataField, AutoNetworkedField]
    public int Count;
}
