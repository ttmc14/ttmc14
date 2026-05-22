using System.Linq;
using System.Text;
using Content.Server.Chat.Managers;
using Content.Server.Voting;
using Content.Server.Voting.Managers;
using Content.Server.Maps;
using Content.Shared.GameTicking;
using JetBrains.Annotations;
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

    [Dependency] private readonly IChatManager _chat = null!;
    [Dependency] private readonly IPrototypeManager _prototype = null!;
    [Dependency] private readonly IVoteManager _vote = null!;
    [Dependency] private readonly IRobustRandom _random = null!;

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

    [PublicAPI]
    public void StartMapVote()
    {
        if (VoteableMaps.Count == 0)
            return;

        var options = CollectVoteOptions();
        if (options.Count == 0)
            return;

        var vote = new VoteOptions
        {
            Title = Loc.GetString("mc-next-map-vote-title"),
            Options = options,
            Duration = TimeSpan.FromMinutes(1),
        };

        vote.SetInitiatorOrServer(null);

        _currentVote = _vote.CreateVote(vote);
        _currentVote.OnFinished += (_, args) => ProcessVoteResults(args.Votes);
        _currentVote.OnCancelled += _ => _currentVote = null;
    }

    [PublicAPI]
    public void CancelMapVote()
    {
        _currentVote?.Cancel();
    }

    private List<(string text, object data)> CollectVoteOptions()
    {
        var options = new List<(string text, object data)>();
        foreach (var mapId in VoteableMaps)
        {
            if (!_prototype.TryIndex(mapId, out var mapProto))
                continue;

            options.Add((Loc.GetString(mapProto.MapName), mapId));
        }
        return options;
    }

    private void ProcessVoteResults(List<int> votes)
    {
        _currentVote = null;

        var results = VoteableMaps
            .Zip(votes, (mapId, voteCount) => (mapId, voteCount))
            .ToList();

        var maxVotes = results.Max(r => r.voteCount);
        var winners = results.Where(r => r.voteCount == maxVotes).Select(r => r.mapId).ToList();

        var isTie = winners.Count > 1;
        var pickedMap = isTie ? _random.Pick(winners) : winners.First();

        var announcement = BuildVoteAnnouncement(results, pickedMap, isTie);
        _chat.DispatchServerAnnouncement(announcement);

        SelectedMap = pickedMap;
    }

    private string BuildVoteAnnouncement(List<(ProtoId<GameMapPrototype> mapId, int voteCount)> results, ProtoId<GameMapPrototype> pickedMap, bool isTie)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Loc.GetString("mc-next-map-vote-header"));

        foreach (var res in results)
        {
            if (!_prototype.TryIndex(res.mapId, out var proto))
                continue;

            sb.AppendLine($"{Loc.GetString(proto.MapName)}: {res.voteCount}");
        }

        if (!_prototype.TryIndex(pickedMap, out var winnerProto))
            return sb.ToString();

        var winnerName = Loc.GetString(winnerProto.MapName);

        sb.AppendLine(isTie
            ? Loc.GetString("mc-next-map-vote-tie", ("winner", winnerName))
            : Loc.GetString("mc-next-map-vote-win", ("winner", winnerName)));

        return sb.ToString();
    }
}
