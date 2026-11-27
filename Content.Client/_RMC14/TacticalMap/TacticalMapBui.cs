using System.Linq;
using System.Numerics;
using System.Threading;
using Content.Client._RMC14.UserInterface;
using Content.Shared._RMC14.TacticalMap;
using Robust.Client.UserInterface.Controls;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Client._RMC14.TacticalMap;

/// <summary>Displays a viewer's selected tactical map layer and keeps canvas drafts per layer.</summary>
public abstract class TacticalMapBui(EntityUid owner, Enum uiKey) : RMCPopOutBui<TacticalMapWindow>(owner, uiKey)
{
    protected override TacticalMapWindow? Window { get; set; }
    protected NetEntity? SelectedMap { get; private set; }

    private List<KeyValuePair<int, NetEntity>> _layers = [];
    private int _selectedIndex;
    private readonly Dictionary<NetEntity, (List<TacticalMapLine> Lines, Dictionary<Vector2i, string> Labels)> _drafts = [];
    private TacticalMapLayerDataMsg? _canvasSource;
    private CancellationTokenSource? _layerRequest;
    private string? _currentMapName;

    protected override void Open()
    {
        base.Open();
        Window = this.CreatePopOutableWindow<TacticalMapWindow>();
        var wrapper = Window.Wrapper;
        TabContainer.SetTabTitle(wrapper.MapTab, Loc.GetString("ui-tactical-map-tab-map"));
        TabContainer.SetTabTitle(wrapper.CanvasTab, Loc.GetString("ui-tactical-map-tab-canvas"));
        wrapper.SetLineLimit(EntMan.System<TacticalMapSystem>().LineLimit);
        wrapper.LayerDown.OnPressed += _ => SelectLayer(_selectedIndex - 1);
        wrapper.LayerUp.OnPressed += _ => SelectLayer(_selectedIndex + 1);
        wrapper.UpdateCanvasButton.OnPressed += _ =>
        {
            if (SelectedMap is { } map && _canvasSource is not null)
            {
                SendMessage(new TacticalMapUpdateCanvasMsg(map, [.. wrapper.Canvas.Lines], new(wrapper.Canvas.TacticalLabels)));
            }
        };
        Refresh();
    }

    /// <summary>Updates drawing access and the publication cooldown for this viewer.</summary>
    public abstract void Refresh();

    protected void UpdateControls(bool canDraw, TimeSpan lastUpdate, TimeSpan nextUpdate)
    {
        if (Window is null)
            return;

        var wrapper = Window.Wrapper;
        TabContainer.SetTabVisible(wrapper.CanvasTab, canDraw);
        wrapper.LastUpdateAt = lastUpdate;
        wrapper.NextUpdateAt = nextUpdate;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (Window is null || state is not TacticalMapBuiState data)
            return;

        _currentMapName = data.MapName;
        Window.SetMapEntity(data.MapName);
        var wrapper = Window.Wrapper;
        wrapper.SetMapEntity(data.MapName);
        wrapper.UpdateSquadObjectivesFromState(data.SquadObjectives);

        _layers = data.Layers.OrderBy(pair => pair.Key).ToList();
        wrapper.LayerControls.Visible = _layers.Count > 1;
        var playerMap = PlayerManager.LocalEntity is { } player
            ? EntMan.GetNetEntity(EntMan.GetComponent<TransformComponent>(player).MapUid)
            : null;
        var preferred = SelectedMap ?? playerMap;
        var index = _layers.FindIndex(pair => pair.Value == preferred);
        SelectLayer(index >= 0 ? index : _layers.FindIndex(pair => pair.Key == 0));
    }

    private void SelectLayer(int index)
    {
        if (Window is null)
            return;

        var wrapper = Window.Wrapper;
        NetEntity? map = null;
        if (_layers.Count > 0)
        {
            _selectedIndex = Math.Clamp(index, 0, _layers.Count - 1);
            var (depth, layer) = _layers[_selectedIndex];
            map = layer;
            wrapper.LayerLabel.Text = Loc.GetString("ui-tactical-map-z-level", ("depth", depth));
            wrapper.LayerDown.Disabled = _selectedIndex == 0;
            wrapper.LayerUp.Disabled = _selectedIndex == _layers.Count - 1;
        }

        if (map == SelectedMap)
            return;

        var canvas = wrapper.Canvas;
        if (_canvasSource is { } source &&
            (!canvas.Lines.SequenceEqual(source.Lines) || !LabelsEqual(canvas.TacticalLabels, source.Labels)))
        {
            _drafts[source.Map] = ([.. canvas.Lines], new(canvas.TacticalLabels));
        }

        StopLayerRequest();
        SelectedMap = map;
        _canvasSource = null;
        wrapper.SetLayer(map);
        wrapper.Canvas.Drawing = false;
        if (map is { } selected)
        {
            void RequestLayer() => SendMessage(new TacticalMapSelectLayerMsg(selected));

            _layerRequest = new CancellationTokenSource();
            Timer.SpawnRepeating(TimeSpan.FromSeconds(1), RequestLayer, _layerRequest.Token);
            RequestLayer();
        }
    }

    private void StopLayerRequest()
    {
        _layerRequest?.Cancel();
        _layerRequest?.Dispose();
        _layerRequest = null;
    }

    private static bool LabelsEqual(Dictionary<Vector2i, string> labels, Dictionary<Vector2i, string> other)
        => labels.Count == other.Count && labels.All(pair => other.TryGetValue(pair.Key, out var text) && pair.Value == text);

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (Window is null || message is not TacticalMapLayerDataMsg data || data.Map != SelectedMap)
            return;

        StopLayerRequest();
        var wrapper = Window.Wrapper;
        wrapper.UpdateBlips(data.Blips.Values.ToArray(), data.Blips.Keys.ToArray());
        wrapper.Map.Lines.Clear();
        wrapper.Map.Lines.AddRange(data.Lines);
        wrapper.Map.UpdateTacticalLabels(data.Labels);

        var canvas = wrapper.Canvas;
        if (_canvasSource is null ||
            (canvas.Lines.SequenceEqual(_canvasSource.Lines) && LabelsEqual(canvas.TacticalLabels, _canvasSource.Labels)))
        {
            var (lines, labels) = _drafts.Remove(data.Map, out var draft) ? draft : (data.Lines, data.Labels);
            canvas.Lines.Clear();
            canvas.LineThicknesses.Clear();
            canvas.Lines.AddRange(lines);
            canvas.UpdateTacticalLabels(labels);
            canvas.Drawing = true;
        }

        _canvasSource = data;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && Window is not null)
        {
            StopLayerRequest();
            try
            {
                var settingsManager = IoCManager.Resolve<TacticalMapSettingsManager>();
                var currentSettings = Window.Wrapper.GetCurrentSettings();

                currentSettings.WindowSize = new Vector2(Window.SetSize.X, Window.SetSize.Y);
                currentSettings.WindowPosition = new Vector2(Window.Position.X, Window.Position.Y);

                settingsManager.SaveSettings(currentSettings, _currentMapName);
            }
            catch (Exception ex)
            {
                Logger.GetSawmill("tactical_map_settings").Error($"Failed to save tactical map settings during disposal for map '{_currentMapName}': {ex}");
            }
        }

        base.Dispose(disposing);
    }
}
