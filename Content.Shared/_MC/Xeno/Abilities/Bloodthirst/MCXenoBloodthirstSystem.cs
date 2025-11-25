using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Plasma;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Bloodthirst;

public sealed class MCXenoBloodthirstSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly IPrototypeManager _prototype = null!;

    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;

    [Dependency] private readonly MCXenoHealSystem _mcXenoHeal = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoBloodthirstComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<MCXenoBloodthirstComponent, MeleeHitEvent>(OnMeleeHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoBloodthirstComponent>();
        while (query.MoveNext(out var entity, out var bloodthirstComponent))
        {
            Process((entity, bloodthirstComponent));
        }
    }

    private void Process(Entity<MCXenoBloodthirstComponent> entity)
    {
        if (entity.Comp.LastFightTime == TimeSpan.Zero)
            return;

        if (entity.Comp.LastFightTime + entity.Comp.DecayDelay > _timing.CurTime)
            return;

        if (_mcXenoPlasma.TryRemovePlasma(entity, entity.Comp.DecayPerTick))
        {
            entity.Comp.Disintegrating = false;
            return;
        }

        if (!entity.Comp.Disintegrating)
        {
            entity.Comp.LastFightTime = _timing.CurTime;
            if (_net.IsServer)
                _popup.PopupEntity(Loc.GetString("mc-xeno-ability-bloodthirst-disintegrating"), entity, entity, PopupType.MediumXeno);
            _audio.PlayPredicted(entity.Comp.Sound, entity, entity);
            entity.Comp.Disintegrating = true;
        }

        if (entity.Comp.LastFightTime + entity.Comp.DamageDelay >= _timing.CurTime)
            return;

        var health = _mcXenoHeal.GetHealth(entity);
        var maxHealth = _mcXenoHeal.GetHealthAlive(entity);
        var damage = float.Min(entity.Comp.DamagePerDisintegrating, health + maxHealth - entity.Comp.LowestHealthAllowed);

        _damageable.TryChangeDamage(entity, new DamageSpecifier(_prototype.Index<DamageGroupPrototype>("MCBrute"), FixedPoint2.New(damage)), ignoreResistances: true, interruptsDoAfters: false);
    }

    private void OnDamageChanged(Entity<MCXenoBloodthirstComponent> entity, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased)
            return;

        entity.Comp.LastFightTime = _timing.CurTime;
    }

    private void OnMeleeHit(Entity<MCXenoBloodthirstComponent> entity, ref MeleeHitEvent args)
    {
        entity.Comp.LastFightTime = _timing.CurTime;
    }
}
