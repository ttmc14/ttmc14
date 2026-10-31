using Content.Shared._MC.Xeno.QueueJoin.Events.Net;
using Robust.Shared.Timing;

namespace Content.Client._MC.Xeno.QueueJoin;

public sealed class MCXenoHiveQueueJoinOfferSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;

    private MCXenoHiveQueueJoinOfferWindow? _window;
    private MCXenoHiveQueueJoinOfferEvent? _offer;

    public override void Initialize()
    {
        SubscribeNetworkEvent<MCXenoHiveQueueJoinOfferEvent>(OnOffer);
    }

    public override void Update(float frameTime)
    {
        if (_window is null || _offer is null)
            return;

        if (_window.IsOpen)
            WindowUpdate();

        if (_offer.Expires - _timing.CurTime <= TimeSpan.Zero)
            WindowClose();
    }

    private void OnOffer(MCXenoHiveQueueJoinOfferEvent message, EntitySessionEventArgs args)
    {
        WindowClose();
        _offer = message;

        WindowOpen();
    }

    private void WindowOpen()
    {
        if (_offer is null)
            return;

        _window = new MCXenoHiveQueueJoinOfferWindow();
        _window.HiveLabel.Text = $"Hive: {_offer.HiveName}";
        _window.PositionLabel.Text = $"Queue Position: {_offer.QueuePosition}";

        _window.AcceptButton.OnPressed += _ =>
        {
            RaiseNetworkEvent(new MCXenoHiveQueueJoinOfferAcceptEvent());
            WindowClose();
        };

        _window.DeclineButton.OnPressed += _ =>
        {
            RaiseNetworkEvent(new MCXenoHiveQueueJoinOfferRejectEvent());
            WindowClose();
        };

        _window.OnClose += () =>
        {
            if (_offer is null)
                return;

            RaiseNetworkEvent(new MCXenoHiveQueueJoinOfferRejectEvent());
        };

        _window.OpenCentered();

        WindowUpdate();
    }

    private void WindowClose()
    {
        _offer = null;

        if (_window is null)
            return;

        _window.Close();
        _window = null;
    }

    private void WindowUpdate()
    {
        if (_window is null || _offer is null)
            return;

        var remaining = _offer.Expires - _timing.CurTime;
        if (remaining <= TimeSpan.Zero)
        {
            _window.CountdownLabel.Text = "Offer expired.";
            return;
        }

        _window.CountdownLabel.Text = $"Time remaining: {remaining.TotalSeconds:F0}s";
    }
}
