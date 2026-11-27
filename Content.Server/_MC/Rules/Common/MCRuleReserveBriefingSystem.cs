using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Rules.Common;
using Content.Shared._RMC14.Xenonids;
using Content.Shared.Ghost;

namespace Content.Server._MC.Rules.Common;

public sealed class MCRuleReserveBriefingSystem : GameRuleSystem<MCRuleReserveBriefingComponent>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GetRoundBriefingEvent>(OnGetBriefing);
    }

    private void OnGetBriefing(ref GetRoundBriefingEvent args)
    {
        var query = QueryActiveRules();
        if (!query.MoveNext(out var uid, out _, out var briefing, out _) || !TryComp<MCRuleReinforcementsComponent>(uid, out var reserves))
            return;

        if (args.Reserves is { } counts)
        {
            counts.Marines = reserves.MarineReserve;
            counts.Xenos = reserves.XenoReserve;
            return;
        }

        var both = args.Viewer is null || HasComp<GhostComponent>(args.Viewer);
        var xeno = HasComp<XenoComponent>(args.Viewer);
        if (both || !xeno)
            args.Lines.Add(Loc.GetString(briefing.MarineBriefing, ("count", reserves.MarineReserve)));

        if (both || xeno)
            args.Lines.Add(Loc.GetString(briefing.XenoBriefing, ("count", reserves.XenoReserve)));
    }
}
