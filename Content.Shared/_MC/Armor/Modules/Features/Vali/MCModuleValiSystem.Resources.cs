using Content.Shared._MC.Armor.Modules.Features.Vali.Components;

namespace Content.Shared._MC.Armor.Modules.Features.Vali;

public sealed partial class MCModuleValiSystem
{
    private void AddResource(Entity<MCModuleValiComponent> entity, float amount)
    {
        if (amount < 0)
            return;

        SetResource(entity, entity.Comp.Resource + amount);
    }

    private void RemoveResource(Entity<MCModuleValiComponent> entity, float amount)
    {
        if (amount < 0)
            return;

        SetResource(entity, entity.Comp.Resource - amount);
    }

    private void SetResource(Entity<MCModuleValiComponent> entity, float amount)
    {
        entity.Comp.Resource = float.Clamp(amount, 0, entity.Comp.ResourceMax);
        Dirty(entity);
    }
}
