using Content.Shared._MC.Xeno.Abilities.Warrior.PrimalRoar.Components;
using Content.Shared._MC.Xeno.Abilities.Warrior.PrimalRoar.Events;

namespace Content.Shared._MC.Xeno.Abilities.Warrior.PrimalRoar;

public sealed class MCXenoPrimalRoarSystem : MCXenoAbilitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoPrimalRoarComponent, MCXenoPrimalRoarActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoPrimalRoarComponent> ent, ref MCXenoPrimalRoarActionEvent args)
    {

    }
}
