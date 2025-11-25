using Content.Shared._MC.Nuke.Generator.Components;
using Content.Shared._MC.Nuke.Generator.UI;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;

namespace Content.Client._MC.Nuke.Generator.UI;

[UsedImplicitly]
public sealed class MCNukeDiskGeneratorBui : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entities = default!;

    [ViewVariables]
    private MCNukeDiskGeneratorWindow? _window;

    public MCNukeDiskGeneratorBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCNukeDiskGeneratorWindow>();
        _window.RunButton.OnPressed += _ => SendMessage(new MCNukeDiskGeneratorRunBuiMessage());

        if (!_entities.TryGetComponent<MCNukeDiskGeneratorComponent>(Owner, out var component))
            return;

        _window.OverallProgressBar.ForegroundStyleBoxOverride = new StyleBoxFlat(component.Color);

        Refresh(component.OverallProgress);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is MCNukeDiskGeneratorOverallProgressBuiState overallProgressBuiState)
            Refresh(overallProgressBuiState.Value);
    }

    private void Refresh(FixedPoint2 value)
    {
        if (_window is null)
            return;

        _window.OverallProgressBar.Value = value.Float() * 100;
        _window.OverallProgressLabel.Text = $"{(value * 100).Int()}%";
    }
}
