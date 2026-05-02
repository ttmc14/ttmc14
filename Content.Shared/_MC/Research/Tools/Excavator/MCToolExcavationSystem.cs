using Content.Shared._RMC14.Marines.Skills;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;

namespace Content.Shared._MC.Research.Tools.Excavator;

public sealed class MCToolExcavationSystem : EntitySystem
{
    private static readonly LocId NotSkilledLocId = "mc-excavation-not-skilled";
    private static readonly LocId ExcavatingLocId = "mc-excavation-excavating";
    private static readonly LocId FoundLocId = "mc-excavation-found";
    private static readonly LocId NothingLocId = "mc-excavation-nothing";

    [Dependency] private readonly SkillsSystem _skills = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = null!;

    [Dependency] private readonly MCExcavationSiteSystem _mcExcavationSite = null!;

    private EntityQuery<MCExcavationSiteComponent> _siteQuery;

    public override void Initialize()
    {
        base.Initialize();

        _siteQuery = GetEntityQuery<MCExcavationSiteComponent>();

        SubscribeLocalEvent<MCToolExcavationComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<MCToolExcavationComponent, MCToolExcavationDoAfterEvent>(OnDoAfter);
    }

    private void OnUseInHand(Entity<MCToolExcavationComponent> entity, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var skill = _skills.GetSkill(args.User, entity.Comp.SkillId);

        if (skill < entity.Comp.SkillLevel)
        {
            _popup.PopupClient(Loc.GetString(NotSkilledLocId), args.User, PopupType.MediumCaution);
            return;
        }

        args.Handled = true;

        _popup.PopupPredicted(Loc.GetString(ExcavatingLocId, ("user", args.User)), args.User, args.User, PopupType.Medium);

        var ev = new MCToolExcavationDoAfterEvent();

        var doAfter = new DoAfterArgs(
            EntityManager,
            args.User,
            entity.Comp.ExcavateTime,
            ev,
            entity,
            target: args.User)
        {
            BreakOnMove = true,
            BreakOnDamage = true
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfter(Entity<MCToolExcavationComponent> entity, ref MCToolExcavationDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var userCoords = Transform(args.User).Coordinates;

        var found = false;

        foreach (var uid in _entityLookup.GetEntitiesInRange(userCoords, entity.Comp.SearchRadius))
        {
            if (!_siteQuery.TryGetComponent(uid, out var site))
                continue;

            _mcExcavationSite.ExcavateSite((uid, site));
            QueueDel(uid);

            found = true;

            _popup.PopupPredicted(Loc.GetString(FoundLocId, ("user", args.User)), args.User, args.User, PopupType.Medium);
            break;
        }

        if (found)
            return;

        _popup.PopupPredicted(Loc.GetString(NothingLocId, ("user", args.User)), args.User, args.User, PopupType.MediumCaution);
    }
}
