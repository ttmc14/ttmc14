using Content.Shared._MC.Electrical.PowerCell.Legacy;
using Content.Shared._MC.Medical.Revive;
using Content.Shared._RMC14.Marines.Skills;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Shared._MC.Medical.Defibrillator;

public sealed partial class MCDefibrillatorSystem
{
    // Managers
    [Dependency] private readonly ISharedPlayerManager _player = null!;

    // Systems
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly InventorySystem _inventory = null!;
    [Dependency] private readonly ItemToggleSystem _toggle = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = null!;
    [Dependency] private readonly SharedMindSystem _mind = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly UseDelaySystem _useDelay = null!;
    [Dependency] private readonly SharedRottingSystem _rotting = null!;

    // RMC
    [Dependency] private readonly SkillsSystem _rmcSkills = null!;

    // MC
    [Dependency] private readonly MCReviveSharedSystem _mcRevive = null!;
    [Dependency] private readonly MCPowerCellProviderSharedSystem _mcPowerCellLegacy = null!;
}
