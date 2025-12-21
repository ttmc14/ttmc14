namespace Content.Shared._MC.Xeno.Abilities.Defiler.Defile;

// TODO: [MC] Use MCXenoAbilitySystem<TComponent, TEvent>
public sealed class MCXenoDefileSystem : MCXenoAbilitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoDefileComponent, MCXenoDefileActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoDefileComponent> entity, ref MCXenoDefileActionEvent args)
    {
        throw new NotImplementedException();
    }
}
