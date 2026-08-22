using Content.Shared._MC.Line;
using Content.Shared._MC.Power.Systems.Providing;
using Content.Shared._MC.Weapon.Laser.Components;
using Content.Shared._MC.Weapons.AttackModeSelection.Core.Events;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Weapon.Laser.Systems;

public sealed partial class MCWeaponLaserSystem : EntitySystem
{
    public static readonly VerbCategory Modes =
        new("mc-verb-categories-weapon-laser-modes", "/Textures/Interface/AdminActions/tricks.png", iconsOnly: false) { Columns = 1 };

    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedContainerSystem _container = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedPhysicsSystem _physics = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedProjectileSystem _projectile = null!;

    [Dependency] private readonly SharedGunSystem _gun = null!;

    [Dependency] private readonly MCLineSystem _mcLine = null!;
    [Dependency] private readonly MCProvidingSharedBatterySystem _mcProvidingBattery = null!;

    public override void Initialize()
    {
        base.Initialize();

        InitializeAppearance();

        SubscribeLocalEvent<MCWeaponLaserComponent, ComponentStartup>(OnStartup);

        SubscribeLocalEvent<MCWeaponLaserComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<MCWeaponLaserComponent, MCAttackModeSelectionEvent>(OnModeSelect);

        SubscribeLocalEvent<MCWeaponLaserComponent, TakeAmmoEvent>(OnTakeAmmo);
        SubscribeLocalEvent<MCWeaponLaserComponent, GetAmmoCountEvent>(OnGetAmmoCount);
    }

    private void OnStartup(Entity<MCWeaponLaserComponent> entity, ref ComponentStartup args)
    {
        SetMode(entity, entity.Comp.StartingMode);
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

    private void OnModeSelect(Entity<MCWeaponLaserComponent> entity, ref MCAttackModeSelectionEvent args)
    {
        SetMode(entity, args.Id);
    }

    private void OnTakeAmmo(Entity<MCWeaponLaserComponent> entity, ref TakeAmmoEvent args)
    {
        args.Ammo.Add((entity, entity.Comp));
    }

    private void OnGetAmmoCount(Entity<MCWeaponLaserComponent> entity, ref GetAmmoCountEvent args)
    {
        UpdateShots(entity);

        args.Count = entity.Comp.Shots;
        args.Capacity = entity.Comp.Shots;
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

        if (!TryComp<GunComponent>(entity, out var gunComponent))
        {
            Log.Error("Not a gun!");
            return;
        }

        gunComponent.FireRate = mode.FireRate;

        _gun.RefreshModifiers(entity.Owner);
    }

    private void UpdateShots(Entity<MCWeaponLaserComponent> entity)
    {
        if (!TryGetAmmo(entity, out var ammoEntity))
            return;

        if (entity.Comp.Mode is not { } mode)
            return;

        if (!_mcProvidingBattery.Supported)
            return;

        var count = _mcProvidingBattery.GetCharge(ammoEntity) / mode.Shots;
        var capacity = _mcProvidingBattery.GetMaxCharge(ammoEntity) / mode.Shots;

        entity.Comp.Shots = (int) count;
        entity.Comp.Capacity = (int) capacity;

        DirtyFields(entity, entity.Comp, null, nameof(MCWeaponLaserComponent.Shots), nameof(MCWeaponLaserComponent.Capacity));
    }
}
