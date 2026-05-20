using System.Linq;
using System.Text;
using Content.Server.Chat.Managers;
using Content.Server.Voting;
using Content.Server.Voting.Managers;
using Content.Shared._MC;
using Content.Shared.GameTicking;
using Content.Server.Maps;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MC.Rules.Crash;

public sealed class MCCrashMapVoteSystem : EntitySystem
{
    private static readonly List<ProtoId<GameMapPrototype>> VoteableMaps = new()
    {
        "MCCanterbury",
        "MCCanterburyMothership",
    };

    [Dependency] private readonly IPrototypeManager _prototype = null!;
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly IConfigurationManager _config = null!;
    [Dependency] private readonly IVoteManager _voteManager = null!;
    [Dependency] private readonly IChatManager _chatManager = null!;

    private IVoteHandle? _currentVote;

    public ProtoId<GameMapPrototype> SelectedMap { get; private set; } = "MCCanterbury";

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        StartMapVote();
    }

    public void StartMapVote()
    {
        if (VoteableMaps.Count == 0)
            return;

        var options = new List<(string text, object data)>();
        foreach (var mapId in VoteableMaps)
        {
            if (!_prototype.TryIndex(mapId, out var mapProto))
                continue;

            options.Add((Loc.GetString(mapProto.MapName), mapId));
        }

        var vote = new VoteOptions
        {
            Title = Loc.GetString("mc-next-map-vote-title"),
            Options = options,
            Duration = TimeSpan.FromMinutes(1),
        };

        vote.SetInitiatorOrServer(null);

        _currentVote = _voteManager.CreateVote(vote);
        _currentVote.OnFinished += (_, args) =>
        {
            _currentVote = null;
            ProtoId<GameMapPrototype> pickedMap;

            var results = VoteableMaps
                .Zip(args.Votes, (mapId, votes) => (mapId, votes))
                .ToList();

            var maxVotes = results.Max(r => r.votes);
            var winners = results.Where(r => r.votes == maxVotes).Select(r => r.mapId).ToList();

            var sb = new StringBuilder();
            sb.AppendLine(Loc.GetString("mc-next-map-vote-header"));

            foreach (var res in results)
            {
                if (!_prototype.TryIndex(res.mapId, out var proto))
                    continue;

                sb.AppendLine($"{Loc.GetString(proto.MapName)}: {res.votes}");
            }

            if (winners.Count > 1)
            {
                pickedMap = _random.Pick(winners);
                if (_prototype.TryIndex(pickedMap, out var proto))
                    sb.AppendLine(Loc.GetString("mc-next-map-vote-tie", ("winner", Loc.GetString(proto.MapName))));
            }
            else
            {
                pickedMap = winners.First();
                if (_prototype.TryIndex(pickedMap, out var proto))
                    sb.AppendLine(Loc.GetString("mc-next-map-vote-win", ("winner", Loc.GetString(proto.MapName))));
            }

            _chatManager.DispatchServerAnnouncement(sb.ToString());
            SelectedMap = pickedMap;
        };

        _currentVote.OnCancelled += _ => _currentVote = null;
    }

    public void CancelMapVote()
    {
        _currentVote?.Cancel();
    }
}
