using System.Linq;
using System.Text;
using Content.Server.Chat.Managers;
using Content.Server.Voting;
using Content.Server.Voting.Managers;
using Content.Shared._MC._Config;
using Content.Shared._RMC14.Rules;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MC.Xeno.Spawn;

public sealed partial class MCXenoSpawnSystem
{
    [Dependency] private readonly IVoteManager _voteManager = null!;
    [Dependency] private readonly IChatManager _chatManager = null!;

    private readonly Dictionary<EntProtoId<RMCPlanetMapPrototypeComponent>, int> _carryoverVotes = new();
    private IVoteHandle? _currentVote;

    // Config values
    private bool _voteEnabled;
    private bool _voteCarryover;
    private int _voteExcludeLast;

    private void InitializeVote()
    {
        Subs.CVar(_config, MCConfigVars.VoteEnabled, v => _voteEnabled = v, true);
        Subs.CVar(_config, MCConfigVars.VoteExcludeLast, v => _voteExcludeLast = v, true);
        Subs.CVar(_config, MCConfigVars.VoteCarryover, v => _voteCarryover = v, true);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        StartPlanetVote();
        ResetPlanet();
    }

    private void StartPlanetVote()
    {
        if (!_voteEnabled)
            return;

        var candidates = _rmcPlanet.GetCandidatesInRotation();
        if (candidates.Count == 0)
        {
            Log.Warning("Cannot start planet vote: no candidates available.");
            return;
        }

        PrepareVoteState(candidates);

        var filteredCandidates = FilterExcludedPlanets(candidates);
        if (filteredCandidates.Count == 0)
        {
            Log.Warning("All planet candidates are excluded by history. Falling back to random selection without vote.");
            _selectedPlanetMap = GetRandomPlanet();
            return;
        }

        var voteOptions = BuildVoteOptions(filteredCandidates);
        CreateAndStartVote(voteOptions, filteredCandidates);
    }

    private void PrepareVoteState(List<RMCPlanet> candidates)
    {
        if (!_voteCarryover)
        {
            foreach (var planet in candidates)
            {
                _carryoverVotes[planet.Proto.ID] = 0;
            }
        }
    }

    private List<RMCPlanet> FilterExcludedPlanets(List<RMCPlanet> candidates)
    {
        return candidates.Where(p => !_lastPlanetMaps.Contains(p.Proto.ID)).ToList();
    }

    private List<(string Text, object Data)> BuildVoteOptions(List<RMCPlanet> planets)
    {
        var options = new List<(string text, object data)>();
        foreach (var planet in planets)
        {
            var name = planet.Proto.Name;
            var carryover = _carryoverVotes.GetValueOrDefault(planet.Proto.ID);

            if (carryover > 0)
                name = $"{name} [+{carryover}]";

            options.Add((name, planet.Comp.Map.ToString()));
        }
        return options;
    }

    private void CreateAndStartVote(List<(string Text, object Data)> options, List<RMCPlanet> validPlanets)
    {
        var vote = new VoteOptions
        {
            Title = Loc.GetString("rmc-distress-signal-next-map-title"),
            Options = options,
            Duration = TimeSpan.FromMinutes(1),
        };
        vote.SetInitiatorOrServer(null);

        _currentVote = _voteManager.CreateVote(vote);

        _currentVote.OnFinished += (_, args) => HandleVoteFinished(args, validPlanets);
        _currentVote.OnCancelled += _ => _currentVote = null;
    }

    private void HandleVoteFinished(VoteFinishedEventArgs args, List<RMCPlanet> validPlanets)
    {
        _currentVote = null;

        var results = CalculateVoteResults(validPlanets, args.Votes);
        var winner = DetermineWinner(results);

        AnnounceResults(results, winner);
        UpdateCarryoverVotes(validPlanets, args.Votes, winner);

        _selectedPlanetMap = winner;
    }

    private List<(RMCPlanet Planet, int NewVotes, int TotalVotes)> CalculateVoteResults(
        List<RMCPlanet> planets,
        IReadOnlyList<int> newVotes)
    {
        return planets.Zip(newVotes,
            (planet, votes) => (
            Planet: planet,
            NewVotes: votes,
            TotalVotes: votes + _carryoverVotes.GetValueOrDefault(planet.Proto.ID)
            ))
            // ReSharper disable once UseCollectionExpression
            .ToList();
    }

    private RMCPlanet DetermineWinner(List<(RMCPlanet Planet, int NewVotes, int TotalVotes)> results)
    {
        var maxVotes = results.Max(r => r.TotalVotes);
        var winners = results.Where(r => r.TotalVotes == maxVotes).Select(r => r.Planet).ToList();

        if (winners.Count > 1)
        {
            Log.Info($"Vote tie between {winners.Count} planets. Randomizing...");
            return _random.Pick(winners);
        }

        return winners.First();
    }

    private void AnnounceResults(
        List<(RMCPlanet Planet, int NewVotes, int TotalVotes)> results,
        RMCPlanet winner)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Loc.GetString("rmc-distress-signal-next-map-header"));

        foreach (var result in results)
        {
            var locKey = result.NewVotes > 0
                ? "rmc-distress-signal-next-map-votes-new"
                : "rmc-distress-signal-next-map-votes";

            sb.AppendLine(Loc.GetString(locKey,
                ("map", result.Planet.Proto.Name),
                ("votes", result.TotalVotes),
                ("newVotes", result.NewVotes)));
        }

        sb.AppendLine(Loc.GetString("rmc-distress-signal-next-map-win", ("winner", winner.Proto.Name)));
        _chatManager.DispatchServerAnnouncement(sb.ToString());
    }

    private void UpdateCarryoverVotes(
        List<RMCPlanet> planets,
        IReadOnlyList<int> newVotes,
        RMCPlanet winner)
    {
        foreach (var (planet, votes) in planets.Zip(newVotes))
        {
            var id = planet.Proto.ID;
            _carryoverVotes[id] = _voteCarryover
                ? _carryoverVotes.GetValueOrDefault(id) + votes
                : 0;
        }

        // Reset winner's carryover to prevent snowballing
        _carryoverVotes[winner.Proto.ID] = 0;
    }

    public void CancelPlanetVote()
    {
        _currentVote?.Cancel();
        _currentVote = null;
    }
}
