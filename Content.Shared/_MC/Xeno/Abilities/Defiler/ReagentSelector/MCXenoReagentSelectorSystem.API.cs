using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector;

public sealed partial class MCXenoReagentSelectorSystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = null!;

    public EntProtoId? GetSmoke(Entity<MCXenoReagentSelectorComponent?> entity)
    {
        return !Resolve(entity, ref entity.Comp)
            ? null
            : entity.Comp.SelectedEntry?.SmokeEntityId;
    }

    public ProtoId<ReagentPrototype>? GetReagent(Entity<MCXenoReagentSelectorComponent?> entity)
    {
        return !Resolve(entity, ref entity.Comp)
            ? null
            : entity.Comp.SelectedEntry?.ReagentId;
    }

    public bool TryInjectReagent(
        Entity<MCXenoReagentSelectorComponent?> entity,
        EntityUid targetUid,
        float amount,
        string solutionId = "chemicals")
    {
        var reagentId = GetReagent(entity);
        if (reagentId is null)
            return false;

        return _solutionContainer.TryGetSolution(targetUid, solutionId, out var solution) &&
               _solutionContainer.TryAddReagent(solution.Value, reagentId.Value, amount);
    }
}
