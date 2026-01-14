using Content.Shared.Actions;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.CreateBomb;

public sealed partial class MCXenoCreateGlobActionEvent : InstantActionEvent
{
    [DataField]
    public string StatePrefix = "create_bomb_count_";
}
