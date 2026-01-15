using Content.Shared.Actions;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.CreateBomb;

public sealed partial class MCXenoCreateGlobActionEvent : InstantActionEvent
{
    [DataField]
    public int Amount = 1;

    [DataField]
    public string StatePrefix = "create_bomb_count_";
}
