using System.Linq;
using Content.Shared.NPC.Prototypes;
using Content.Server.Actions;
using Content.Server.Body.Systems;
using Content.Server.Chat;
using Content.Server.Chat.Systems;
using Content.Server.Emoting.Systems;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Speech.EntitySystems;
using Content.Server.Roles;
using Content.Shared.Anomaly.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Armor;
using Content.Shared.Bed.Sleep;
using Content.Shared.Cloning.Events;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared._MC.Zombies;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._MC.Zombies;

public sealed partial class MCZombieSystem : SharedMCZombieSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly AutoEmoteSystem _autoEmote = default!;
    [Dependency] private readonly EmoteOnDamageSystem _emoteOnDamage = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;

    public readonly ProtoId<NpcFactionPrototype> Faction = "Zombie";

    public const SlotFlags ProtectiveSlots =
        SlotFlags.FEET |
        SlotFlags.HEAD |
        SlotFlags.EYES |
        SlotFlags.GLOVES |
        SlotFlags.MASK |
        SlotFlags.NECK |
        SlotFlags.INNERCLOTHING |
        SlotFlags.OUTERCLOTHING;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCZombieComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCZombieComponent, EmoteEvent>(OnEmote, before:
            new[] { typeof(VocalSystem), typeof(BodyEmotesSystem) });

        SubscribeLocalEvent<MCZombieComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<MCZombieComponent, MobStateChangedEvent>(OnMobState);
        SubscribeLocalEvent<MCZombieComponent, CloningEvent>(OnZombieCloning);
        SubscribeLocalEvent<MCZombieComponent, TryingToSleepEvent>(OnSleepAttempt);
        SubscribeLocalEvent<MCZombieComponent, GetCharactedDeadIcEvent>(OnGetCharacterDeadIC);
        SubscribeLocalEvent<MCZombieComponent, GetCharacterUnrevivableIcEvent>(OnGetCharacterUnrevivableIC);
        SubscribeLocalEvent<MCZombieComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<MCZombieComponent, MindRemovedMessage>(OnMindRemoved);

        SubscribeLocalEvent<MCPendingZombieComponent, MapInitEvent>(OnPendingMapInit);
        SubscribeLocalEvent<MCPendingZombieComponent, BeforeRemoveAnomalyOnDeathEvent>(OnBeforeRemoveAnomalyOnDeath);

        SubscribeLocalEvent<MCIncurableZombieComponent, MapInitEvent>(OnPendingMapInit);

        SubscribeLocalEvent<MCZombifyOnDeathComponent, MobStateChangedEvent>(OnDamageChanged);
    }

    private void OnBeforeRemoveAnomalyOnDeath(Entity<MCPendingZombieComponent> ent, ref BeforeRemoveAnomalyOnDeathEvent args)
    {
        args.Cancelled = true;
    }

    private void OnPendingMapInit(EntityUid uid, MCIncurableZombieComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.Action, component.ZombifySelfActionPrototype);
        _faction.AddFaction(uid, Faction);

        if (HasComp<MCZombieComponent>(uid) || HasComp<MCZombieImmuneComponent>(uid))
            return;

        EnsureComp<MCPendingZombieComponent>(uid, out MCPendingZombieComponent pendingComp);
        pendingComp.GracePeriod = _random.Next(pendingComp.MinInitialInfectedGrace, pendingComp.MaxInitialInfectedGrace);
    }

    private void OnPendingMapInit(EntityUid uid, MCPendingZombieComponent component, MapInitEvent args)
    {
        if (_mobState.IsDead(uid))
        {
            MCZombifyEntity(uid);
            return;
        }

        component.NextTick = _timing.CurTime + TimeSpan.FromSeconds(1f);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var curTime = _timing.CurTime;

        // Hurt the living infected
        var query = EntityQueryEnumerator<MCPendingZombieComponent, DamageableComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out var comp, out var damage, out var mobState))
        {
            if (comp.NextTick > curTime)
                continue;

            comp.NextTick = curTime + TimeSpan.FromSeconds(1f);
            comp.GracePeriod -= TimeSpan.FromSeconds(1f);
            if (comp.GracePeriod > TimeSpan.Zero)
                continue;

            if (_random.Prob(comp.InfectionWarningChance))
                _popup.PopupEntity(Loc.GetString(_random.Pick(comp.InfectionWarnings)), uid, uid);
            // Damage is applied through MCBlackGoo reagent metabolism, not here
        }

        // Heal the zombified
        var zombQuery = EntityQueryEnumerator<MCZombieComponent, DamageableComponent, MobStateComponent>();
        while (zombQuery.MoveNext(out var uid, out var comp, out var damage, out var mobState))
        {
            if (comp.NextTick + TimeSpan.FromSeconds(1) > curTime)
                continue;

            comp.NextTick = curTime;

            if (_mobState.IsDead(uid, mobState))
                continue;

            var multiplier = _mobState.IsCritical(uid, mobState)
                ? comp.PassiveHealingCritMultiplier
                : 1f;

            _damageable.TryChangeDamage(uid, comp.PassiveHealing * multiplier, true, false, damage);
        }
    }

    private void OnSleepAttempt(EntityUid uid, MCZombieComponent component, ref TryingToSleepEvent args)
    {
        args.Cancelled = true;
    }

    private void OnGetCharacterDeadIC(EntityUid uid, MCZombieComponent component, ref GetCharactedDeadIcEvent args)
    {
        args.Dead = true;
    }

    private void OnGetCharacterUnrevivableIC(EntityUid uid, MCZombieComponent component, ref GetCharacterUnrevivableIcEvent args)
    {
        args.Unrevivable = true;
    }

    private void OnStartup(EntityUid uid, MCZombieComponent component, ComponentStartup args)
    {
        if (component.EmoteSoundsId == null)
            return;
        _protoManager.TryIndex(component.EmoteSoundsId, out component.EmoteSounds);
    }

    private void OnEmote(EntityUid uid, MCZombieComponent component, ref EmoteEvent args)
    {
        if (args.Handled)
            return;
        args.Handled = _chat.TryPlayEmoteSound(uid, component.EmoteSounds, args.Emote);
    }

    private void OnMobState(EntityUid uid, MCZombieComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive)
        {
            EnsureComp<EmoteOnDamageComponent>(uid);
            _emoteOnDamage.AddEmote(uid, "Scream");

            EnsureComp<AutoEmoteComponent>(uid);
            _autoEmote.AddEmote(uid, "ZombieGroan");
        }
        else
        {
            _emoteOnDamage.RemoveEmote(uid, "Scream");
            _autoEmote.RemoveEmote(uid, "ZombieGroan");
        }
    }

    private float GetMCZombieInfectionChance(EntityUid uid, MCZombieComponent zombieComponent)
    {
        var chance = zombieComponent.BaseZombieInfectionChance;

        var armorEv = new CoefficientQueryEvent(ProtectiveSlots);
        RaiseLocalEvent(uid, armorEv);
        foreach (var resistanceEffectiveness in zombieComponent.ResistanceEffectiveness.DamageDict)
        {
            if (armorEv.DamageModifiers.Coefficients.TryGetValue(resistanceEffectiveness.Key, out var coefficient))
            {
                var adjustedCoefficient = 1 - ((1 - coefficient) * resistanceEffectiveness.Value.Float());
                chance *= adjustedCoefficient;
            }
        }

        var zombificationResistanceEv = new MCZombificationResistanceQueryEvent(ProtectiveSlots);
        RaiseLocalEvent(uid, zombificationResistanceEv);
        chance *= zombificationResistanceEv.TotalCoefficient;

        return MathF.Max(chance, zombieComponent.MinZombieInfectionChance);
    }

    private void OnMeleeHit(EntityUid uid, MCZombieComponent component, MeleeHitEvent args)
    {
        if (!TryComp<MCZombieComponent>(args.User, out _))
            return;

        if (!args.HitEntities.Any())
            return;

        foreach (var entity in args.HitEntities)
        {
            if (args.User == entity)
                continue;

            if (!TryComp<MobStateComponent>(entity, out var mobState))
                continue;

            if (HasComp<MCZombieComponent>(entity))
            {
                args.BonusDamage = -args.BaseDamage;
            }
            else
            {
                if (!HasComp<MCZombieImmuneComponent>(entity) && !HasComp<MCNonSpreaderZombieComponent>(args.User) && _random.Prob(GetMCZombieInfectionChance(entity, component)))
                {
                    EnsureComp<MCPendingZombieComponent>(entity);
                    EnsureComp<MCZombifyOnDeathComponent>(entity);
                }
            }

            if (_mobState.IsIncapacitated(entity, mobState) && !HasComp<MCZombieComponent>(entity) && !HasComp<MCZombieImmuneComponent>(entity))
            {
                MCZombifyEntity(entity);
                args.BonusDamage = -args.BaseDamage;
            }
            else if (mobState.CurrentState == MobState.Alive)
            {
                _damageable.TryChangeDamage(uid, component.HealingOnBite, true, false);
            }
        }
    }

    public void MCZombifyEntity(EntityUid target, MobStateComponent? mobState = null)
    {
        // MC zombification logic here
        if (HasComp<MCZombieComponent>(target))
            return;

        EnsureComp<MCZombieComponent>(target);
        RemComp<MCPendingZombieComponent>(target);
        RemComp<MCZombifyOnDeathComponent>(target);

        var ev = new MCEntityZombifiedEvent(target);
        RaiseLocalEvent(ref ev);
    }

    private void OnZombieCloning(EntityUid uid, MCZombieComponent component, ref CloningEvent args)
    {
        // Handle cloning
    }

    private void OnMindAdded(EntityUid uid, MCZombieComponent component, ref MindAddedMessage args)
    {
        // Handle mind added
    }

    private void OnMindRemoved(EntityUid uid, MCZombieComponent component, ref MindRemovedMessage args)
    {
        // Handle mind removed
    }

    // Damage change handling moved to MCZombieSystem.Transform.cs
}
