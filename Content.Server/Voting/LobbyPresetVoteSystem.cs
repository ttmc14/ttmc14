using Content.Server.GameTicking;
using Content.Server.Voting.Managers;

namespace Content.Server.Voting;

public sealed class LobbyPresetVoteSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _ticker = null!;
    [Dependency] private readonly IVoteManager _votes = null!;

    private IVoteHandle? _vote;
    private bool _mapsLoading;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlayerJoinedLobbyEvent>(OnPlayerJoinedLobby);
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
        SubscribeLocalEvent<LoadingMapsEvent>(OnLoadingMaps);
    }

    private void OnPlayerJoinedLobby(PlayerJoinedLobbyEvent args)
    {
        if (_vote is not null || _mapsLoading || !_ticker.LobbyEnabled || _ticker.DummyTicker ||
            _ticker.RunLevel != GameRunLevel.PreRoundLobby)
            return;

        _vote = _votes.TryCreateLobbyPresetVote();
    }

    private void OnRunLevelChanged(GameRunLevelChangedEvent args)
    {
        if (args.New != GameRunLevel.PreRoundLobby)
            return;

        CancelVote();
        _mapsLoading = false;
    }

    private void OnLoadingMaps(LoadingMapsEvent args)
    {
        _mapsLoading = true;
        CancelVote();
    }

    private void CancelVote()
    {
        if (_vote is { Finished: false })
        {
            _vote.Cancel();
        }

        _vote = null;
    }
}
