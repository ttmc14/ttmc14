using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter;

namespace Content.Shared._MC.Xeno.Abilities.Sentinel.NeurotoxinSting;

public sealed class MCXenoNeurotoxinStingSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoNeurotoxinStingComponent, MCXenoNeurotoxinStingActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoNeurotoxinStingComponent, MCXenoNeurotoxinStingDoAfterEvent>(OnActionDoAfter);
    }

    private void OnAction(Entity<MCXenoNeurotoxinStingComponent> entity, ref MCXenoNeurotoxinStingActionEvent args)
    {
        if (args.Handled || !TryUseAction(entity, args.Action, args.Target))
            return;

        args.Handled = true;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, new MCXenoNeurotoxinStingDoAfterEvent(args.Action, EntityManager), entity, args.Target, entity)
        {
            DistanceThreshold = entity.Comp.Range,
            RequireCanInteract = false,
        });
    }

    private void OnActionDoAfter(Entity<MCXenoNeurotoxinStingComponent> entity, ref MCXenoNeurotoxinStingDoAfterEvent args)
    {
        if (args.Handled || args.Target is not {} target)
            return;

        args.Handled = true;

        if (args.Injects < entity.Comp.Count - 1)
        {
            args.Injects++;
            args.Repeat = true;
        }

        AnimateHit(entity, target);

        if (!_solution.TryGetSolution(target, entity.Comp.Solution, out var solution, out _))
            return;

        foreach (var reagentQuantity in entity.Comp.Reagents)
        {
            _solution.TryAddReagent(solution.Value, reagentQuantity.Reagent, reagentQuantity.Quantity, out _);
        }
    }
}
