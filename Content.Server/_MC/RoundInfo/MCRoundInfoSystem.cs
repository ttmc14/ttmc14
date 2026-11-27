using Content.Server.EUI;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Presets;
using Content.Shared._MC.RoundInfo;
using Content.Shared.Alert;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._MC.RoundInfo;

public sealed class MCRoundInfoSystem : EntitySystem
{
    private static readonly ProtoId<AlertPrototype> InfoAlert = "MCRoundInfo";

    [Dependency] private readonly AlertsSystem _alerts = null!;
    [Dependency] private readonly EuiManager _eui = null!;
    [Dependency] private readonly GameTicker _ticker = null!;

    [Dependency] private readonly ISharedPlayerManager _players = null!;

    private readonly Dictionary<ICommonSession, MCRoundInfoEui> _windows = [];

    public override void Initialize()
    {
        SubscribeLocalEvent<PlayerAttachedEvent>(OnAttached);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnDetached);
        SubscribeLocalEvent<ActorComponent, MCOpenRoundInfoAlertEvent>(OnAlert);
        SubscribeLocalEvent<RoundInfoUpdatedEvent>(_ => Refresh());
        SubscribeLocalEvent<GameRunLevelChangedEvent>(_ => Refresh());
    }

    private void OnAttached(PlayerAttachedEvent args)
    {
        RefreshPlayer(args.Player, args.Entity, GetInfoPreset());
    }

    private void OnDetached(PlayerDetachedEvent args)
    {
        _alerts.ClearAlert(args.Entity, InfoAlert);
        if (_windows.TryGetValue(args.Player, out var window))
        {
            window.Close();
        }
    }

    private void OnAlert(Entity<ActorComponent> actor, ref MCOpenRoundInfoAlertEvent args)
    {
        var session = actor.Comp.PlayerSession;
        if (args.Handled || session.AttachedEntity != actor.Owner || GetInfoPreset() is not { } preset)
            return;

        args.Handled = true;
        if (_windows.ContainsKey(session))
            return;

        var window = new MCRoundInfoEui(this, GetState(actor.Owner, preset));
        _windows.Add(session, window);
        _eui.OpenEui(window, session);
        window.StateDirty();
    }

    private GamePresetPrototype? GetInfoPreset()
    {
        if (_ticker.RunLevel != GameRunLevel.InRound || _ticker.CurrentPreset is not { RoundInfoLayout: not null } preset)
            return null;

        return !string.IsNullOrWhiteSpace(preset.Description) &&
            Loc.TryGetString(preset.Description, out var text) && !string.IsNullOrWhiteSpace(text) ? preset : null;
    }

    private void Refresh()
    {
        var preset = GetInfoPreset();
        foreach (var session in _players.Sessions)
        {
            if (session.AttachedEntity is { } viewer)
            {
                RefreshPlayer(session, viewer, preset);
            }
        }
    }

    private void RefreshPlayer(ICommonSession session, EntityUid viewer, GamePresetPrototype? preset)
    {
        _windows.TryGetValue(session, out var window);
        if (preset is null)
        {
            _alerts.ClearAlert(viewer, InfoAlert);
            window?.Close();
            return;
        }

        EnsureComp<AlertsComponent>(viewer);
        _alerts.ShowAlert(viewer, InfoAlert);
        window?.UpdateState(GetState(viewer, preset));
    }

    private MCRoundInfoState GetState(EntityUid viewer, GamePresetPrototype preset)
    {
        var layout = preset.RoundInfoLayout!.Value;
        var state = new MCRoundInfoState(layout, preset.ModeTitle, preset.Description, []);
        var briefing = new GetRoundBriefingEvent(viewer, state.Briefing,
            layout == MCRoundInfoLayout.Warfare ? state.Reserves : null);
        RaiseLocalEvent(ref briefing);
        return state;
    }

    internal void OnClosed(ICommonSession session) => _windows.Remove(session);
}
