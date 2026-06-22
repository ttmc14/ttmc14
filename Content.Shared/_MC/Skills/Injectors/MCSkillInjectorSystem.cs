using Content.Shared._MC.Skills.Injectors.Components;
using Content.Shared._RMC14.Marines.Skills;
using Content.Shared.Interaction.Events;

namespace Content.Shared._MC.Skills.Injectors;

public sealed class MCSkillInjectorSystem : EntitySystem
{
    [Dependency] private readonly SkillsSystem _rmcSkills = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCSkillInjectorComponent, UseInHandEvent>(OnUse);
    }

    private void OnUse(Entity<MCSkillInjectorComponent> entity, ref UseInHandEvent args)
    {
        args.Handled = true;

        if (!TryComp<MCSkillInjectableComponent>(args.User,  out var injectableComponent))
            return;

        if (injectableComponent.SlotsFilled >= injectableComponent.SlotsMax)
            return;

        var value = _rmcSkills.GetSkill(args.User, entity.Comp.Skill);
        if (value >= entity.Comp.LevelMax)
            return;

        var toValue = int.Clamp(value + entity.Comp.Level, 0, entity.Comp.LevelMax);
        _rmcSkills.SetSkill(args.User, entity.Comp.Skill, toValue);

        injectableComponent.SlotsFilled++;
        DirtyField(args.User, injectableComponent, nameof(MCSkillInjectableComponent.SlotsFilled));

        PredictedQueueDel(entity.Owner);
    }
}
