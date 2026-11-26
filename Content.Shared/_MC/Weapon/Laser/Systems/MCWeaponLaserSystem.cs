using Content.Shared._MC.Line;
using Content.Shared._MC.Power.Battery;
using Content.Shared._MC.Power.Battery.Components;
using Content.Shared._MC.Power.Battery.Events;
using Content.Shared._MC.Weapon.Laser.Components;
using Content.Shared._MC.Weapon.Laser.Events;
using Content.Shared._MC.Weapons.AttackModeSelection.Core.Events;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Weapon.Laser.Systems;

public sealed partial class MCWeaponLaserSystem : EntitySystem
{
    private static readonly VerbCategory Modes =
        new("mc-verb-categories-weapon-laser-modes", "/Textures/Interface/AdminActions/tricks.png", iconsOnly: false) { Columns = 1 };

    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedContainerSystem _container = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedPhysicsSystem _physics = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedProjectileSystem _projectile = null!;

    [Dependency] private readonly SharedGunSystem _gun = null!;

    [Dependency] private readonly MCLineSystem _mcLine = null!;
    [Dependency] private readonly MCBatterySystem _mcBattery = null!;


    /// <summary>
    /// Registers handlers for laser startup, firing mode selection, ammunition queries and appearance.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();

        InitializeAppearance();

        SubscribeLocalEvent<MCWeaponLaserAmmoComponent, MCBatteryChargeChangedEvent>(OnCellChargeChanged,
            after: [typeof(MCBatteryVisualsSystem)]);

        SubscribeLocalEvent<MCWeaponLaserComponent, ComponentStartup>(OnStartup);

        SubscribeLocalEvent<MCWeaponLaserComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<MCWeaponLaserComponent, MCAttackModeSelectionEvent>(OnModeSelect);

        SubscribeLocalEvent<MCWeaponLaserComponent, TakeAmmoEvent>(OnTakeAmmo);
        SubscribeLocalEvent<MCWeaponLaserComponent, GetAmmoCountEvent>(OnGetAmmoCount);
    }

    private void OnStartup(Entity<MCWeaponLaserComponent> entity, ref ComponentStartup args)
    {
        SetMode(entity, entity.Comp.StartingMode);
        UpdateShots(entity);
        UpdateAppearance(entity);
    }

    private void OnGetVerbs(Entity<MCWeaponLaserComponent> entity, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        foreach (var (key, mode) in entity.Comp.Modes)
        {
            args.Verbs.Add(new Verb
            {
                Text = key,
                Icon = mode.Icon,
                Category = Modes,
                Act = () =>
                {
                    SetMode(entity, key);
                },
            });
        }
    }

    private void OnModeSelect(Entity<MCWeaponLaserComponent> entity, ref MCAttackModeSelectionEvent args) =>
        SetMode(entity, args.Id);

    private void OnTakeAmmo(Entity<MCWeaponLaserComponent> entity, ref TakeAmmoEvent args) =>
        args.Ammo.Add((entity, entity.Comp));

    private void OnGetAmmoCount(Entity<MCWeaponLaserComponent> entity, ref GetAmmoCountEvent args)
    {
        UpdateShots(entity);

        args.Count = entity.Comp.Shots;
        args.Capacity = entity.Comp.Capacity;
    }

    private void OnCellChargeChanged(Entity<MCWeaponLaserAmmoComponent> cell, ref MCBatteryChargeChangedEvent args)
    {
        if (_net.IsClient)
            return;

        if (float.Abs(args.Charge - args.PreviousCharge) < 1e-5f || args.Charge >= args.MaxCharge)
            return;

        var gun = Transform(cell).ParentUid;
        if (!TryComp<MCWeaponLaserComponent>(gun, out var laser))
            return;

        var ev = new MCLaserCellChargeChangedEvent(gun, cell.Owner);
        RaiseLocalEvent(Transform(gun).ParentUid, ref ev);

        if (ev.Recharged && TryComp<MCBatteryComponent>(cell.Owner, out var battery))
            UpdateShots((gun, laser), battery);
    }

    private bool TryGetAmmo(Entity<MCWeaponLaserComponent> entity, out Entity<MCWeaponLaserAmmoComponent> ammoEntity)
    {
        ammoEntity = default;

        if (!_container.TryGetContainer(entity, entity.Comp.ContainerId, out var container))
            return false;

        if (!container.ContainedEntities.TryFirstOrNull(out var uid) || !TryComp<MCWeaponLaserAmmoComponent>(uid, out var component))
            return false;

        ammoEntity = (uid.Value, component);
        return true;
    }

    private void SetMode(Entity<MCWeaponLaserComponent> entity, string modeKey)
    {
        if (!entity.Comp.Modes.TryGetValue(modeKey, out var mode))
        {
            Log.Error($"Mode {modeKey} not found");
            return;
        }

        entity.Comp.Mode = mode;
        DirtyField(entity, entity.Comp, nameof(MCWeaponLaserComponent.Mode));

        UpdateShots(entity);

        if (!TryComp<GunComponent>(entity, out var gunComponent))
        {
            Log.Error("Not a gun!");
            return;
        }

        gunComponent.FireRate = mode.FireRate;
        Dirty(entity, gunComponent);

        _gun.RefreshModifiers(entity.Owner);
    }

    /// <summary>
    /// Resolves the battery currently installed in the gun's laser cell slot.
    /// </summary>
    public bool TryGetInstalledBattery(Entity<MCWeaponLaserComponent?> gun, out Entity<MCBatteryComponent> cell)
    {
        cell = default;
        if (!Resolve(gun, ref gun.Comp, false) || !TryGetAmmo((gun.Owner, gun.Comp), out var ammo) ||
            !TryComp<MCBatteryComponent>(ammo.Owner, out var battery))
            return false;

        cell = (ammo.Owner, battery);
        return true;
    }

    private void UpdateShots(Entity<MCWeaponLaserComponent> entity)
    {
        if (!TryGetInstalledBattery(entity.AsNullable(), out var cell))
            return;

        UpdateShots(entity, cell.Comp);
    }

    /// <summary>
    /// Updates the shot counter using the charge of the installed cell and the current firing mode.
    /// </summary>
    private void UpdateShots(Entity<MCWeaponLaserComponent> entity, MCBatteryComponent battery)
    {
        if (entity.Comp.Mode is not { } mode)
            return;

        var shotCost = battery.MaxCharge / mode.Shots;
        entity.Comp.Shots = (int) (battery.Charge / shotCost);
        entity.Comp.Capacity = mode.Shots;

        DirtyFields(entity, entity.Comp, null, nameof(MCWeaponLaserComponent.Shots), nameof(MCWeaponLaserComponent.Capacity));
    }
}
