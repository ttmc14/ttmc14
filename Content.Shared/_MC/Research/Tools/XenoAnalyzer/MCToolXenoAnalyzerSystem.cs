using Content.Shared._RMC14.Marines.Skills;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Random;

namespace Content.Shared._MC.Research.Tools.XenoAnalyzer;

public sealed class MCToolXenoAnalyzerSystem : EntitySystem
{
    private static readonly LocId CantResearchLocId = "mc-xeno-analyzer-cant-research";
    private static readonly LocId AlreadyProbedLocId = "mc-xeno-analyzer-already-probed";
    private static readonly LocId BeginsCuttingLocId = "mc-xeno-analyzer-begins-cutting";
    private static readonly LocId FindingWeakPointLocId = "mc-xeno-analyzer-finding-weak-point";
    private static readonly LocId AnalyzeFailedLocId = "mc-xeno-analyzer-failed";

    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SkillsSystem _skills = null!;

    private EntityQuery<MCToolXenoAnalyzableComponent> _analyzableQuery;

    public override void Initialize()
    {
        base.Initialize();

        _analyzableQuery = GetEntityQuery<MCToolXenoAnalyzableComponent>();

        SubscribeLocalEvent<MCToolXenoAnalyzerComponent, AfterInteractEvent>(OnInteract);
        SubscribeLocalEvent<MCToolXenoAnalyzerComponent, MCToolXenoAnalyzerInteractDoAfter>(OnInteractDoAfter);
    }

    private void OnInteract(Entity<MCToolXenoAnalyzerComponent> entity, ref AfterInteractEvent args)
    {
        if (args.Target is not { } target)
            return;

        if (!TryGetValidTarget(target, args.User))
            return;

        args.Handled = true;

        var duration = GetAnalyzeDuration(entity, args.User);
        var popupText = GetAnalyzeMessage(entity, args.User, target);

        ShowAnalyzePopup(args.User, popupText);
        StartAnalyzeDoAfter(entity, args.User, target, duration);
    }

    private void OnInteractDoAfter(Entity<MCToolXenoAnalyzerComponent> entity, ref MCToolXenoAnalyzerInteractDoAfter args)
    {
        if (args.Handled || args.Cancelled || args.Target is null)
            return;

        if (!_analyzableQuery.TryGetComponent(args.Target.Value, out var target))
            return;

        if (target.Researched)
            return;

        if (RollAnalyzeFailure(entity, args.User))
        {
            ShowAnalyzePopup(args.User,  Loc.GetString(AnalyzeFailedLocId, ("user", args.User)), PopupType.MediumCaution);
            return;
        }

        CompleteResearch(args.Target.Value, target);
    }

    private bool TryGetValidTarget(EntityUid targetUid, EntityUid user)
    {
        if (!_analyzableQuery.TryGetComponent(targetUid, out var target))
        {
            _popup.PopupClient(Loc.GetString(CantResearchLocId), user, PopupType.SmallCaution);
            return false;
        }

        if (!target.Researched)
            return true;

        _popup.PopupClient(Loc.GetString(AlreadyProbedLocId), user, PopupType.SmallCaution);
        return false;
    }

    private TimeSpan GetAnalyzeDuration(Entity<MCToolXenoAnalyzerComponent> entity, EntityUid user)
    {
        var skill = _skills.GetSkill(user, entity.Comp.SkillId);

        return skill >= entity.Comp.SkillLevel
            ? entity.Comp.BaseAnalyzeTime
            : entity.Comp.FailedAnalyzeTime - entity.Comp.SkillTimeReduction * skill;
    }

    private string GetAnalyzeMessage(Entity<MCToolXenoAnalyzerComponent> entity, EntityUid user, EntityUid target)
    {
        var skill = _skills.GetSkill(user, entity.Comp.SkillId);

        return Loc.GetString(skill >= entity.Comp.SkillLevel
            ? BeginsCuttingLocId
            : FindingWeakPointLocId,
            ("user", user),
            ("target", target));
    }

    private void ShowAnalyzePopup(EntityUid user, string text, PopupType type = PopupType.Medium)
    {
        _popup.PopupPredicted(text, user, user, type);
    }

    private void StartAnalyzeDoAfter(Entity<MCToolXenoAnalyzerComponent> entity, EntityUid user, EntityUid target, TimeSpan duration)
    {
        var ev = new MCToolXenoAnalyzerInteractDoAfter();
        var doAfter = new DoAfterArgs(
            EntityManager,
            user,
            duration,
            ev,
            entity,
            target: target);

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void CompleteResearch(EntityUid targetUid, MCToolXenoAnalyzableComponent component)
    {
        var coordinates = Transform(targetUid).Coordinates;

        PredictedSpawnAtPosition(
            component.RewardProtId,
            coordinates);

        component.Researched = true;
        Dirty(targetUid, component);
    }

    private bool RollAnalyzeFailure(Entity<MCToolXenoAnalyzerComponent> analyzer, EntityUid user)
    {
        var chance = GetAnalyzeFailChance(analyzer, user);
        return chance > 0f && _random.Prob(chance);
    }

    private float GetAnalyzeFailChance(Entity<MCToolXenoAnalyzerComponent> entity, EntityUid user)
    {
        var skill = _skills.GetSkill(user, entity.Comp.SkillId);
        var missingSkill = entity.Comp.SkillLevel - skill;

        if (missingSkill <= 0)
            return 0f;

        var failChance = entity.Comp.FailChancePerMissingSkill * missingSkill;
        return float.Min(failChance, entity.Comp.MaxFailChance);
    }
}
