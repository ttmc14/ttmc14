using Content.Shared._MC.Armor.Modules.Core.Components;
using Content.Shared._MC.Armor.Modules.Core.Events;
using Content.Shared._MC.Armor.Modules.Features.Shield.Components;
using Content.Shared._RMC14.Aura;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Armor.Modules.Features.Shield;

public sealed class MCModuleShieldSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedAuraSystem _rmcAura = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCModuleShieldComponent, MCArmorModuleRelayedEvent<ExaminedEvent>>(OnExamined);
        SubscribeLocalEvent<MCModuleShieldComponent, MCArmorModuleRelayedEvent<DamageModifyEvent>>(OnDamage);
        SubscribeLocalEvent<MCModuleShieldComponent, MCArmorModuleUserChangedEvent>(OnUserChanged);
        SubscribeLocalEvent<MCModuleShieldComponent, MCArmorModuleAttachedEvent>(OnAttached);
        SubscribeLocalEvent<MCModuleShieldComponent, MCArmorModuleDetachedEvent>(OnDeattached);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MCModuleShieldComponent>();

        var time = _timing.CurTime;
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.RechargeCooldownEndTime > time)
                continue;

            TryStartRecharge((uid, component));

            if (!component.Recharging)
                continue;

            if (component.NextRechargeTick > time)
                continue;

            component.NextRechargeTick = component.RechargeRate + time;
            component.Health = float.Min(component.Health + component.RechargeAmount, component.HealthMax);

            RefreshAura((uid, component));

            if (component.Health < component.HealthMax)
                continue;

            component.Health = component.HealthMax;
            StopRecharge((uid, component));
        }
    }

    private void OnExamined(Entity<MCModuleShieldComponent> entity, ref MCArmorModuleRelayedEvent<ExaminedEvent> args)
    {
        using (args.Args.PushGroup(nameof(MCModuleShieldComponent)))
        {
            args.Args.PushMarkup(Loc.GetString("mc-shield-examine-recharge-rate", ("rate", entity.Comp.RechargeAmount / entity.Comp.RechargeRate.TotalSeconds)));
            args.Args.PushMarkup(Loc.GetString("mc-shield-examine-current-health", ("current", entity.Comp.Health)));
            args.Args.PushMarkup(Loc.GetString("mc-shield-examine-max-health", ("max", entity.Comp.HealthMax)));

            if (entity.Comp.RechargeCooldownEndTime < _timing.CurTime)
                return;

            var secondsLeft = entity.Comp.RechargeCooldownEndTime - _timing.CurTime;
            args.Args.PushMarkup(Loc.GetString("mc-shield-examine-recharge-delayed", ("time", secondsLeft)));
        }
    }

    private void OnDamage(Entity<MCModuleShieldComponent> entity, ref MCArmorModuleRelayedEvent<DamageModifyEvent> args)
    {
        if (args.Args.Tool is null)
            return;

        var total = args.Args.Damage.GetTotal().Float();
        var left = ApplyShieldDamage(entity, total);
        args.Args.Damage *= left / total;
    }

    private void OnUserChanged(Entity<MCModuleShieldComponent> entity, ref MCArmorModuleUserChangedEvent args)
    {
        if (args.OldUser is { } oldUser)
            RemComp<AuraComponent>(oldUser);

        RefreshAura(entity, args.NewUser, true);
    }

    private void OnAttached(Entity<MCModuleShieldComponent> entity, ref MCArmorModuleAttachedEvent args)
    {
        RefreshAura(entity, args.User, true);
    }

    private void OnDeattached(Entity<MCModuleShieldComponent> entity, ref MCArmorModuleDetachedEvent args)
    {
        if (args.User is { } oldUser)
            RemComp<AuraComponent>(oldUser);
    }

    public float ApplyShieldDamage(Entity<MCModuleShieldComponent> entity, float totalDamage)
    {
        if (entity.Comp.Health <= 0)
            return totalDamage;

        StopRecharge(entity);

        var shieldLeft = entity.Comp.Health - totalDamage;
        if (shieldLeft > 0)
        {
            entity.Comp.Health = shieldLeft;
            RefreshAura(entity);
            StartRechargeCooldown(entity);
            return 0f;
        }

        entity.Comp.Health = 0f;
        RefreshAura(entity);
        StartRechargeCooldown(entity, extraDelay: true);

        _audio.PlayPredicted(entity.Comp.SoundDown, entity, entity);

        return -shieldLeft;
    }

    private void RefreshAura(Entity<MCModuleShieldComponent> entity, EntityUid? user = null, bool force = false)
    {
        if (user is null)
        {
            if (!TryComp<MCArmorModularClothingComponent>(Transform(entity).ParentUid, out var containerComponent))
                return;

            if (containerComponent.CurrentUser is null)
                return;

            user = containerComponent.CurrentUser;
        }

        var newColor = GetShieldColor(entity.Comp);
        if (entity.Comp.CurrentColor == newColor && !force)
            return;

        entity.Comp.CurrentColor = newColor;

        _rmcAura.GiveAura(user.Value, newColor, null);
    }

    private void StartRechargeCooldown(Entity<MCModuleShieldComponent> entity, bool extraDelay = false)
    {
        var extraTime = extraDelay ? TimeSpan.FromSeconds(1) : TimeSpan.Zero;
        entity.Comp.RechargeCooldownEndTime = entity.Comp.RechargeCooldown + extraTime + _timing.CurTime;
    }

    private void TryStartRecharge(Entity<MCModuleShieldComponent> entity)
    {
        if (entity.Comp.Health >= entity.Comp.HealthMax || entity.Comp.Recharging)
            return;

        entity.Comp.Recharging = true;
        _audio.PlayPredicted(entity.Comp.SoundRecharge, entity, entity);
    }

    private void StopRecharge(Entity<MCModuleShieldComponent> entity)
    {
        entity.Comp.Recharging = false;
    }

    private static Color GetShieldColor(MCModuleShieldComponent comp)
    {
        var ratio = comp.Health / comp.HealthMax;
        return ratio switch
        {
            <= 0.33f => comp.ShieldColorLow,
            <= 0.66f => comp.ShieldColorMid,
            _ => comp.ShieldColorFull,
        };
    }
}
