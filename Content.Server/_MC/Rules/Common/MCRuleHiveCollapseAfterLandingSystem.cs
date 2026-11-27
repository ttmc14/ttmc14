using Content.Server.GameTicking.Rules;
using Content.Server.Shuttles.Events;
using Content.Shared._MC.Rules.Common;
using Content.Shared._MC.Xeno.Hive.Collapse;
using Content.Shared._RMC14.Marines;

namespace Content.Server._MC.Rules.Common;

public sealed class MCRuleHiveCollapseAfterLandingSystem : GameRuleSystem<MCRuleHiveCollapseAfterLandingComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlmayerComponent, FTLCompletedEvent>(OnShuttleLanded);
        SubscribeLocalEvent<MCBeforeHiveCollapseUpdateEvent>(OnBeforeHiveCollapseUpdate);
    }

    private void OnShuttleLanded(Entity<AlmayerComponent> entity, ref FTLCompletedEvent args)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var rule, out _))
        {
            if (CompOrNull<MCRuleXenoMapComponent>(uid)?.Map is { } planet && Transform(planet).MapUid == args.MapUid)
                rule.Landed = true;
        }
    }

    private void OnBeforeHiveCollapseUpdate(ref MCBeforeHiveCollapseUpdateEvent args)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out _, out var rule, out _))
        {
            if (rule.Landed)
                continue;

            args.Cancelled = true;
            return;
        }
    }
}
