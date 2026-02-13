using System.Text;
using Content.Shared._MC.Utilities;
using Content.Shared._MC.Weapon.Vali.Components;
using Content.Shared._RMC14.Chemistry.Reagent;
using Content.Shared.Actions;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Examine;
using Content.Shared.Weapons.Melee;

namespace Content.Shared._MC.Weapon.Vali;

public sealed partial class MCWeaponValiSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly SharedActionsSystem _actions = null!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = null!;
    [Dependency] private readonly RMCReagentSystem _rmcReagentSystem = null!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeActions();
        InitializeInjection();
        InitializeMelee();
        InitializeUsage();

        SubscribeLocalEvent<MCWeaponValiComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<MCWeaponValiComponent> entity, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(MCWeaponValiComponent), entity.Comp.ExamineGroupPriority))
        {
            var stringBuilder = new StringBuilder();

            var attackRate = TryComp<MeleeWeaponComponent>(entity, out var melee) ? melee.AttackRate : 1;

            stringBuilder.AppendLine(Loc.GetString("mc-weapon-vali-examine-reagent-usage",
                ("amount", entity.Comp.ReagentUsage),
                ("perSecond", (entity.Comp.ReagentUsage.Float() * attackRate).ToString("F1"))
            ));

            stringBuilder.AppendLine(Loc.GetString("mc-weapon-vali-examine-harvest-amount",
                ("amount", entity.Comp.HarvestAmount),
                ("perSecond", (entity.Comp.HarvestAmount * attackRate).ToString("F1"))
            ));

            if (entity.Comp.Reagents.Count > 0)
            {
                stringBuilder.AppendLine(Loc.GetString("mc-weapon-vali-hold"));
                foreach (var (reagentId, value) in entity.Comp.Reagents)
                {
                    if (!_rmcReagentSystem.TryIndex(reagentId, out var reagent))
                        continue;

                    stringBuilder.AppendLine(Loc.GetString("mc-weapon-vali-reagent-amount",
                        ("reagent", GetReagentString(reagent)),
                        ("amount", value)
                    ));
                }
            }

            stringBuilder.AppendLine(Loc.GetString("mc-weapon-vali-compatible"));
            foreach (var (reagentId, _) in entity.Comp.ReagentData)
            {
                if (!_rmcReagentSystem.TryIndex(reagentId, out var reagent))
                    continue;

                stringBuilder.AppendLineReagent(reagent);
            }

            args.PushMarkup(stringBuilder.ToString());
        }
    }

    private static string GetReagentString(ReagentPrototype reagent)
    {
        return $"[color={reagent.SubstanceColor.ToHexNoAlpha()}]{reagent.LocalizedName}[/color]";
    }
}
