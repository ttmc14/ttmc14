using Content.Shared._MC.Damage.Integrity.Systems;
using Content.Shared._MC.Engineering.Sentries.Components;
using Content.Shared._MC.Engineering.Sentries.UI;

namespace Content.Shared._MC.Engineering.Sentries;

public sealed class MCSentryUISystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = null!;
    [Dependency] private readonly MCIntegritySystem _mcIntegrity = null!;

    public override void Initialize() =>
        SubscribeLocalEvent<MCSentryComponent, BoundUIOpenedEvent>(OnUIOpen);

    public void Refresh(Entity<MCSentryComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        RefreshState((entity, entity.Comp));
    }

    private void OnUIOpen(Entity<MCSentryComponent> entity, ref BoundUIOpenedEvent args) =>
        RefreshState(entity);

    private void RefreshState(Entity<MCSentryComponent> entity) =>
        _ui.SetUiState(entity.Owner, MCSentryUI.Key, GenerateBui(entity));

    private MCSentryBuiState GenerateBui(Entity<MCSentryComponent> entity)
    {
        var state = new MCSentryBuiState();

        if (TryComp<MCSentryAlertComponent>(entity, out var alertComponent))
            state.AlertMode = alertComponent.AlertMode;

        if (TryComp<MCSentryRadialComponent>(entity, out var radialComponent))
            state.RadialMode = radialComponent.RadialMode;

        if (TryComp<MCSentryRoundsComponent>(entity, out var roundsComponent))
        {
            state.Rounds = roundsComponent.Rounds;
            state.MaxRounds = roundsComponent.MaxRounds;
        }

        state.Integrity = _mcIntegrity.GetIntegrityPercentage(entity.Owner, "Destruction");
        return state;
    }
}
