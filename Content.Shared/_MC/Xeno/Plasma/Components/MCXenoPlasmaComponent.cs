using Content.Shared._MC.Xeno.Plasma.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Plasma.Components;

/// <summary>
/// Controls which external effects may replenish or drain this xenomorph's reserves.
/// Ability costs are paid from the same pool regardless of drain immunity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoPlasmaSystem))]
public sealed partial class MCXenoPlasmaComponent : Component
{
    /// <summary>
    /// Displays the blood reserve against its full capacity on the in-world xenomorph HUD.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ShowBloodHud;

    /// <summary>
    /// Uses red for the regenerating portion of the in-world plasma bar.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool UseRedPlasmaBar;

    /// <summary>
    /// Whether other xenomorphs can replenish this reserve through a plasma transfer.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CanBeGivenPlasma = true;

    /// <summary>
    /// Whether plasma-draining smoke and weapon hits leave this reserve intact.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ImmuneToPlasmaDrain;
}
