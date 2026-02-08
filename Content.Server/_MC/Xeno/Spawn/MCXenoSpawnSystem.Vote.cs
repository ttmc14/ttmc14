using System.Linq;
using System.Text;
using Content.Server.Chat.Managers;
using Content.Server.Voting;
using Content.Server.Voting.Managers;
using Content.Shared._MC;
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

    // Config
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
        ResetSelectedPlanet();
    }

    private void StartPlanetVote()
    {
        if (!_voteEnabled)
            return;

        var planets = _rmcPlanet.GetCandidatesInRotation();
        if (!_voteCarryover)
        {
            foreach (var planet in planets)
            {
                _carryoverVotes[planet.Proto.ID] = 0;
            }
        }

        planets.RemoveAll(p => _lastPlanetMaps.Contains(p.Proto.ID));

        var options = new List<(string text, object data)>();
        foreach (var planet in planets)
        {
            var name = planet.Proto.Name;
            var votes = _carryoverVotes.GetValueOrDefault(planet.Proto.ID);
            if (votes > 0)
                name = $"{name} [+{votes}]";

            options.Add((name, planet.Comp.Map.ToString()));
        }

        var vote = new VoteOptions
        {
            Title = Loc.GetString("rmc-distress-signal-next-map-title"),
            Options = options,
            Duration = TimeSpan.FromMinutes(1),
        };
        vote.SetInitiatorOrServer(null);

        _currentVote = _voteManager.CreateVote(vote);
        _currentVote.OnFinished += (_, args) =>
        {
            _currentVote = null;
            RMCPlanet picked;

            var adjustedVotes = planets
                .Zip(args.Votes, (planet, newVotes) => (
                    planet,
                    newVotes,
                    totalVotes: newVotes + _carryoverVotes.GetValueOrDefault(planet.Proto.ID)
                ))
                .ToList();
            var maxVotes = adjustedVotes.Max(v => v.totalVotes);
            var winningMaps = adjustedVotes
                .Where(v => v.totalVotes == maxVotes)
                .Select(v => v.planet)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine(Loc.GetString("rmc-distress-signal-next-map-header"));
            foreach (var result in adjustedVotes)
            {
                sb.AppendLine(Loc.GetString(result.newVotes > 0
                    ? "rmc-distress-signal-next-map-votes-new"
                    : "rmc-distress-signal-next-map-votes",
                    ("map", result.planet.Proto.Name),
                    ("votes", result.totalVotes),
                    ("newVotes", result.newVotes)));
            }

            if (winningMaps.Count > 1)
            {
                sb.AppendLine(Loc.GetString("rmc-distress-signal-next-map-tiebreaker"));
                foreach (var map in winningMaps)
                {
                    sb.AppendLine($"    {map.Proto.Name}");
                }
                picked = _random.Pick(winningMaps);
            }
            else
            {
                picked = winningMaps.First();
            }
            sb.AppendLine(Loc.GetString("rmc-distress-signal-next-map-win", ("winner", picked.Proto.Name)));

            _chatManager.DispatchServerAnnouncement(sb.ToString());

            foreach (var (planet, votes) in planets.Zip(args.Votes))
            {
                var id = planet.Proto.ID;
                _carryoverVotes[id] = _voteCarryover ? _carryoverVotes.GetValueOrDefault(id) + votes : 0;
            }

            _carryoverVotes[picked.Proto.ID] = 0;
            _selectedPlanetMap = picked;
        };

        _currentVote.OnCancelled += _ => _currentVote = null;
    }

    public void CancelPlanetVote()
    {
        _currentVote?.Cancel();
    }
}
