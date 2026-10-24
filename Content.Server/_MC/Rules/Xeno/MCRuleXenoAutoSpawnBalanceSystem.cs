using Content.Server._MC.Xeno;
using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Rules.Xeno;
using Content.Shared.GameTicking.Components;

namespace Content.Server._MC.Rules.Xeno;

public sealed class MCRuleXenoAutoSpawnBalanceSystem : GameRuleSystem<MCRuleXenoAutoSpawnBalanceComponent>
{
    [Dependency] private readonly MCXenoAutoSpawnBalanceSystem _mcXenoAutoSpawnBalance = null!;

    protected override void Started(EntityUid uid,
        MCRuleXenoAutoSpawnBalanceComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        _mcXenoAutoSpawnBalance.Enable();
    }

    protected override void Ended(EntityUid uid,
        MCRuleXenoAutoSpawnBalanceComponent component,
        GameRuleComponent gameRule,
        GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        _mcXenoAutoSpawnBalance.Disable();
    }
}
