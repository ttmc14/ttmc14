using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared._MC.Rules.Common;
using Content.Shared.GameTicking;

namespace Content.Server._MC.Rules.Common;

public sealed class MCRuleReinforcementsSystem : GameRuleSystem<MCRuleReinforcementsComponent>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCRuleReinforcementsComponent, MCPopulationCheckedEvent>(OnPopulationChecked);

        SubscribeLocalEvent<PlayerBeforeSpawnEvent>(OnBeforeSpawn);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete);
    }

    private void OnPopulationChecked(Entity<MCRuleReinforcementsComponent> rule, ref MCPopulationCheckedEvent args)
    {
        if (GameTicker.RunLevel != GameRunLevel.InRound)
            return;

        var growth = new MCGetReserveGrowthEvent();
        RaiseLocalEvent(rule.Owner, ref growth);

        rule.Comp.MarineReserve = (int) float.Min(int.MaxValue, rule.Comp.MarineReserve + growth.Marines);
        rule.Comp.XenoReserve = (int) float.Min(int.MaxValue, rule.Comp.XenoReserve + growth.Xenos);

        var larvae = new MCIssueReserveLarvaeEvent(args, rule.Comp.XenoReserve);
        RaiseLocalEvent(rule.Owner, ref larvae);

        rule.Comp.XenoReserve -= larvae.Issued;

        if (growth.Marines != 0 || growth.Xenos != 0 || larvae.Issued != 0)
            GameTicker.UpdateInfoText();
    }

    private void OnBeforeSpawn(PlayerBeforeSpawnEvent args)
    {
        var query = QueryActiveRules();
        if (args.LateJoin && query.MoveNext(out _, out _, out var rule, out _) && rule.MarineReserve <= 0)
            args.SpawnError = rule.NoMarines;
    }

    private void OnSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        var query = QueryActiveRules();
        if (!args.LateJoin || !query.MoveNext(out _, out _, out var rule, out _))
            return;

        rule.MarineReserve--;
        GameTicker.UpdateInfoText();
    }
}

[ByRefEvent]
public struct MCGetReserveGrowthEvent
{
    public long Marines;
    public long Xenos;
}

[ByRefEvent]
public record struct MCIssueReserveLarvaeEvent(MCPopulationCheckedEvent Population, int Available)
{
    public int Issued;
}
