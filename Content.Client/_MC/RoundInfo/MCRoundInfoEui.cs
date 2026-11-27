using System.Numerics;
using Content.Client.Eui;
using Content.Shared._MC.RoundInfo;
using Content.Shared.Eui;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client._MC.RoundInfo;

[UsedImplicitly]
public sealed class MCRoundInfoEui : BaseEui
{
    private readonly DefaultWindow _window = new();
    private readonly ScrollContainer _scroll = new() { HScrollEnabled = false, ReturnMeasure = true };
    private MCRoundInfoLayout? _layout;
    private MCRoundInfoView? _view;

    public MCRoundInfoEui()
    {
        _window.Contents.AddChild(_scroll);
        _window.OnClose += OnClose;
    }

    public override void Closed()
    {
        _window.OnClose -= OnClose;
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not MCRoundInfoState info)
            return;

        var first = _view is null;
        if (_view is null || _layout != info.Layout)
        {
            if (_view is not null)
            {
                _scroll.RemoveChild(_view);
            }

            _view = info.Layout switch
            {
                MCRoundInfoLayout.Warfare => new MCWarfareRoundInfoView(),
                _ => new MCStandardRoundInfoView(),
            };
            _layout = info.Layout;
            _scroll.AddChild(_view);
            _window.MinSize = Vector2.Min(_view.WindowSize, _window.UserInterfaceManager.WindowRoot.Size);
        }

        _window.Title = info.Layout == MCRoundInfoLayout.Standard ? Loc.GetString(info.Title) : Loc.GetString("mc-round-info-title");
        _view.ShowState(info);
        if (first)
        {
            _window.SetWidth = _window.MinWidth;
            _window.Measure(_window.UserInterfaceManager.WindowRoot.Size);
            _window.SetSize = _window.DesiredSize;
            _window.OpenCentered();
        }
    }

    private void OnClose() => SendMessage(new CloseEuiMessage());
}
