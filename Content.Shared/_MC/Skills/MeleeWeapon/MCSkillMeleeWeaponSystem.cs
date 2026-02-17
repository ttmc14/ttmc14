using Content.Shared._RMC14.Marines.Skills;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Skills.MeleeWeapon;

public sealed class MCSkillMeleeWeaponSystem : EntitySystem
{
    private static readonly EntProtoId<SkillDefinitionComponent> SkillMeleeWeapons = "MCSkillMeleeWeapons";
    private static readonly EntProtoId<SkillDefinitionComponent> SkillCqc = "MCSkillCqc";

    private static readonly float SkillMeleeWeaponBuff = 0.15f;
    private static readonly float SkillCqcBuff = 0f;

    [Dependency] private readonly SkillsSystem _rmcSkills = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GetMeleeDamageEvent>(OnGetMeleeDamage);
        SubscribeLocalEvent<GetMeleeAttackRateEvent>(OnGetMeleeAttackRate);
    }

    private void OnGetMeleeDamage(ref GetMeleeDamageEvent ev)
    {
        var skill = _rmcSkills.GetSkill(ev.User, SkillMeleeWeapons);
        ev.Damage += ev.Damage * SkillMeleeWeaponBuff * skill;
    }

    private void OnGetMeleeAttackRate(ref GetMeleeAttackRateEvent ev)
    {
        var skill = _rmcSkills.GetSkill(ev.User, SkillCqc);
        ev.Rate += SkillCqcBuff * skill;
    }
}
