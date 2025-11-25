using Content.Shared._MC.Stun.Events;
using Content.Shared._MC.Xeno.Abilities.Endure;
using Content.Shared._MC.Xeno.Abilities.Pounce;
using Content.Shared._MC.Xeno.Abilities.Ravage;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._RMC14.Aura;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Network;

namespace Content.Shared._MC.Xeno.Abilities.Rage;

public sealed class MCXenoRageSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly SharedPopupSystem _sharedPopup = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = null!;

    [Dependency] private readonly XenoPlasmaSystem _rmcXenoPlasma = null!;
    [Dependency] private readonly SharedAuraSystem _rmcAura = null!;

    [Dependency] private readonly MCXenoHealSystem _mcXenoHeal = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoRageComponent, DamageChangedEvent>(OnDamageChanged);

        SubscribeLocalEvent<MCXenoRageActiveComponent, DamageChangedEvent>(OnActiveDamageChanged);
        SubscribeLocalEvent<MCXenoRageActiveComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCXenoRageActiveComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<MCXenoRageActiveComponent, MCStunAttemptEvent>(OnStunAttempt);
        SubscribeLocalEvent<MCXenoRageActiveComponent, MCStaggerAttemptEvent>(OnStaggerAttempt);
        SubscribeLocalEvent<MCXenoRageActiveComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
        SubscribeLocalEvent<MCXenoRageActiveComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
        SubscribeLocalEvent<MCXenoRageActiveComponent, MeleeHitEvent>(OnMeleeHit);
        // TODO: slowdown immune
    }

    private void OnDamageChanged(Entity<MCXenoRageComponent> entity, ref DamageChangedEvent args)
    {
        if (CheckHealthThreshold(entity, entity.Comp.MinHealthThreshold))
            return;

        if (HasComp<MCXenoRageActiveComponent>(entity))
            return;

        EnsureComp<MCXenoRageActiveComponent>(entity);
    }

    private void OnActiveDamageChanged(Entity<MCXenoRageActiveComponent> entity, ref DamageChangedEvent args)
    {
        if (!CheckHealthThreshold(entity, entity.Comp.MinHealthThreshold))
        {
            var health = _mcXenoHeal.GetHealth(entity);
            var maxHealth = _mcXenoHeal.GetMaxHealth(entity);
            var maxHealthAlive = _mcXenoHeal.GetHealthAlive(entity);
            var endureHealthLimit = maxHealthAlive - maxHealth;
            var rageThreshold = maxHealth * (1 - entity.Comp.MinHealthThreshold);

            entity.Comp.RagePower = float.Max(0, 1 - (health - endureHealthLimit) / (maxHealth - endureHealthLimit - rageThreshold));
            _movementSpeedModifier.RefreshMovementSpeedModifiers(entity);

            if (health >= 0 || entity.Comp.OnCooldown)
                return;

            if (_net.IsServer)
                _sharedPopup.PopupEntity(Loc.GetString("mc-xeno-ability-rage-rip-and-tear"), entity, entity, PopupType.LargeCaution);

            _rmcXenoPlasma.RegenPlasma(entity.Owner, CompOrNull<XenoPlasmaComponent>(entity)?.MaxPlasma ?? 0);

            ClearUseDelay<MCXenoRavageActionEvent>(entity);
            ClearUseDelay<MCXenoPounceActionEvent>(entity);

            entity.Comp.OnCooldown = true;
            return;
        }

        RemCompDeferred<MCXenoRageActiveComponent>(entity);
        RemCompDeferred<AuraComponent>(entity);
    }

    private void OnStartup(Entity<MCXenoRageActiveComponent> entity, ref ComponentStartup args)
    {
        if (_net.IsClient)
            return;

        _rmcAura.GiveAura(entity, entity.Comp.AuraColor, null, entity.Comp.AuraStrength);
    }

    private void OnRemove(Entity<MCXenoRageActiveComponent> entity, ref ComponentRemove args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(entity);
    }

    private void OnStunAttempt(Entity<MCXenoRageActiveComponent> entity, ref MCStunAttemptEvent args)
    {
        if (CheckHealthThreshold(entity, entity.Comp.StaggerStunImmuneThreshold))
            return;

        args.Canceled = true;
    }

    private void OnStaggerAttempt(Entity<MCXenoRageActiveComponent> entity, ref MCStaggerAttemptEvent args)
    {
        if (CheckHealthThreshold(entity, entity.Comp.StaggerStunImmuneThreshold))
            return;

        args.Canceled = true;
    }

    private void OnRefreshMovementSpeed(Entity<MCXenoRageActiveComponent> entity,
        ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(1 + entity.Comp.RagePower * entity.Comp.SpeedModifier);
    }

    private void OnGetMeleeDamage(Entity<MCXenoRageActiveComponent> entity, ref GetMeleeDamageEvent args)
    {
        args.Damage *= 1 + entity.Comp.RagePower;
    }

    private void OnMeleeHit(Entity<MCXenoRageActiveComponent> entity, ref MeleeHitEvent args)
    {
        foreach (var uid in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(uid))
                continue;

            if (TryComp<MCXenoEndureActiveComponent>(entity, out var endureActive))
                endureActive.EndTime += TimeSpan.FromSeconds(float.Max(1, 2 * entity.Comp.RagePower));

            _mcXenoHeal.Heal(entity, entity.Comp.HealPerSlash + entity.Comp.HealPerSlash * entity.Comp.RagePower);
        }
    }

    private bool CheckHealthThreshold(EntityUid uid, float threshold)
    {
        var health = _mcXenoHeal.GetHealth(uid);
        var maxHealth = _mcXenoHeal.GetHealthAlive(uid);
        return health > maxHealth * threshold;
    }
}
