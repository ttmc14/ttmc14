using Content.Server.GameTicking.Rules;
using Content.Shared._MC.ASRS.Systems;
using Content.Shared._MC.Rules.Common;
using Content.Shared.GameTicking.Components;

namespace Content.Server._MC.Rules.Common;

public sealed class MCRuleAsrsSystem : GameRuleSystem<MCRuleASRSComponent>
{
    [Dependency] private readonly MCASRSSystem _asrs = null!;

    protected override void Started(EntityUid uid, MCRuleASRSComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        foreach (var (account, balance) in component.StartingBalances)
            _asrs.SetBalance(balance, account);
    }
}
