using Content.Shared._MC.Medical.SkillInjectors.Components;
using Content.Shared._RMC14.Marines.Skills;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Medical.SkillInjectors;

public sealed class MCSkillInjectorSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = null!;
    [Dependency] private readonly SkillsSystem _rmcSkills = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCSkillInjectorComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<MCSkillInjectorComponent, ExaminedEvent>(OnExamined);
    }

    private void OnUse(Entity<MCSkillInjectorComponent> entity, ref UseInHandEvent args)
    {
        args.Handled = true;

        if (!TryComp<MCSkillInjectableComponent>(args.User,  out var injectableComponent))
            return;

        if (injectableComponent.SlotsFilled >= injectableComponent.SlotsMax)
            return;

        if (!CheckRequirements(entity, args.User))
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

    private bool CheckRequirements(Entity<MCSkillInjectorComponent> entity, EntityUid uid)
    {
        if (entity.Comp.RequirementSkill is not { } skillId || entity.Comp.RequirementLevel is not { } skillLevel)
            return true;

        return _rmcSkills.GetSkill(uid, skillId) >= skillLevel;
    }

    private void OnExamined(Entity<MCSkillInjectorComponent> entity, ref ExaminedEvent args)
    {
        TryPushMessageGain(entity, ref args);
        TryPushMessageRequirements(entity, ref args);
    }

    private void TryPushMessageGain(Entity<MCSkillInjectorComponent> entity, ref ExaminedEvent args)
    {
        if (!_prototype.TryIndex(entity.Comp.Skill, out var skill))
            return;

        var message = new FormattedMessage();
        message.AddMarkupOrThrow(Loc.GetString("mc-skill-injector-examined-additive",
            ("skill", skill.Name),
            ("level", entity.Comp.Level),
            ("levelMax", entity.Comp.LevelMax)
        ));

        args.PushMessage(message);
    }

    private void TryPushMessageRequirements(Entity<MCSkillInjectorComponent> entity, ref ExaminedEvent args)
    {
        if (entity.Comp.RequirementSkill is not { } skillId || entity.Comp.RequirementLevel is not { } skillLevel)
            return;

        if (!_prototype.TryIndex(skillId, out var skill))
            return;

        var message = new FormattedMessage();
        message.AddMarkupOrThrow(Loc.GetString("mc-skill-injector-examined-requirements",
            ("skill", skill.Name),
            ("level", skillLevel)
        ));

        args.PushMessage(message);
    }
}
