using Content.Server.GameTicking.Rules;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared._MC.Nuke.Bomb.Events;
using Content.Shared._MC.Rules.EndRound;
using Content.Shared._RMC14.Rules;

namespace Content.Server._MC.Rules.EndRound;

public sealed class MCRuleEndRoundNukeExplodedSystem : GameRuleSystem<MCRuleEndRoundNukeExplodedComponent>
{
    [Dependency] private readonly MCRuleEndRoundSystem _endRound = null!;
    [Dependency] private readonly CESharedZLevelsSystem _levels = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MCNukeExplodedEvent>(OnDetonated);
    }

    private void OnDetonated(MCNukeExplodedEvent ev)
    {
        var rules = QueryActiveRules();
        while (rules.MoveNext(out var uid, out _, out var rule, out _))
        {
            if (!IsPlanetLevel(Transform(ev.Bomb).MapUid))
            {
                _endRound.EndRound("NukeOffworld");
                return;
            }

            var evacuated = TryComp<MCRuleEndRoundShuttleEvacuationComponent>(uid, out var evacuation) && evacuation.Evacuated;
            _endRound.EndRound(evacuated ? rule.EndRuleId : rule.WithoutEvacuationId ?? rule.EndRuleId);
        }
    }

    private bool IsPlanetLevel(EntityUid? map)
    {
        if (HasComp<RMCPlanetComponent>(map))
            return true;

        return map is { } uid && TryComp<CEZLevelMapComponent>(uid, out var level) &&
            _levels.TryMapOffset((uid, level), -level.Depth, out var surface) && HasComp<RMCPlanetComponent>(surface);
    }
}
