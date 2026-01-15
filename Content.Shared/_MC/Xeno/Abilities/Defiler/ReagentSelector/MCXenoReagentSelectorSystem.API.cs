using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector;

public sealed partial class MCXenoReagentSelectorSystem
{
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
}
