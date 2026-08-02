using Content.Shared._MC.Engineering.Miners.Components;
using Content.Shared.DoAfter;

namespace Content.Shared._MC.Engineering.Miners;

public sealed partial class MCMinerModuleSystem
{
    public void StartInsertingModule(Entity<MCMinerModuleContainerComponent?> entity, EntityUid module, EntityUid user)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        if (!CanInsert((entity.Owner, entity.Comp), module))
            return;

        var duration = GetDurationInsert((entity.Owner, entity.Comp), user);
        var ev = new Events.Equipment.MCMinerModuleAttachedDoAfterEvent();

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, duration, ev, entity, entity, module)
        {
            BlockDuplicate = true,
            BreakOnDropItem = true,
            BreakOnMove = true,
            BreakOnHandChange = true,
        });
    }

    public void StartTakeModule(Entity<MCMinerModuleContainerComponent?> entity, EntityUid user)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        if (!HasModule((entity.Owner, entity.Comp)))
            return;

        var duration = GetDurationTake((entity.Owner, entity.Comp), user);
        var ev = new Events.Equipment.MCMinerModuleDeattachedDoAfterEvent();

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, duration, ev, entity, entity)
        {
            BlockDuplicate = true,
            BreakOnDropItem = true,
            BreakOnMove = true,
            BreakOnHandChange = true,
        });
    }

    private TimeSpan GetDurationInsert(Entity<MCMinerModuleContainerComponent?> entity, EntityUid user)
    {
        if (!Resolve(entity, ref entity.Comp))
            return TimeSpan.Zero;

        var skill = _rmcSkills.GetSkill(user, entity.Comp.SkillId);
        if (skill >= entity.Comp.SkillLevel)
            return TimeSpan.FromSeconds(15) - TimeSpan.FromSeconds((skill - entity.Comp.SkillLevel) * 4);

        return TimeSpan.FromSeconds(30) - TimeSpan.FromSeconds(skill * 3.5f);
    }

    private TimeSpan GetDurationTake(Entity<MCMinerModuleContainerComponent?> entity, EntityUid user)
    {
        if (!Resolve(entity, ref entity.Comp))
            return TimeSpan.Zero;

        var skill = _rmcSkills.GetSkill(user, entity.Comp.SkillId);
        if (skill >= entity.Comp.SkillLevel)
            return TimeSpan.FromSeconds(15) - TimeSpan.FromSeconds((skill - entity.Comp.SkillLevel) * 4);

        return TimeSpan.FromSeconds(30) - TimeSpan.FromSeconds(skill * 3.5f);
    }
}
