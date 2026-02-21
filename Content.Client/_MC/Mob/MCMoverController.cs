using Content.Shared._MC;
using Content.Shared.Alert;
using Content.Shared.CCVar;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Client.Player;
using Robust.Shared.Configuration;

namespace Content.Client._MC.Mob;

public sealed class MCMoverController : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = null!;
    [Dependency] private readonly IPlayerManager _player = null!;
    [Dependency] private readonly AlertsSystem _alerts = null!;

    private bool _showAlert;
    private bool _toggleWalking;

    public override void Initialize()
    {
        base.Initialize();

        _configuration.OnValueChanged(CCVars.ToggleWalk, value => _toggleWalking = value, true);
        _configuration.OnValueChanged(MCConfigVars.QoLRunAlert, value => _showAlert = value, true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_showAlert || _toggleWalking)
            return;

        if (_player.LocalEntity is not {} entity)
            return;

        if (!TryComp<InputMoverComponent>(entity, out var inputMoverComponent))
            return;

        if (!inputMoverComponent.Sprinting)
        {
            _alerts.ShowAlert(entity, SharedMoverController.WalkingAlert, showCooldown: false, autoRemove: false);
            return;
        }

        _alerts.ClearAlert(entity, SharedMoverController.WalkingAlert);
    }
}
