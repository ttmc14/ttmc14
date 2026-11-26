using Content.Shared._MC.Aura;
using Content.Shared._MC.Xeno.Abilities.Gorger.PsychicLink.Components;
using Content.Shared._MC.Xeno.Abilities.Gorger.PsychicLink.Events;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._RMC14.Damage;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.PsychicLink;

public sealed class MCXenoPsychicLinkSystem : MCXenoAbilitySystem
{
    private const string AuraLayer = "psychic-link";

    [Dependency] private readonly MCAuraSystem _aura = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly DamageableSystem _damage = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;

    [Dependency] private readonly MCXenoHealSystem _heal = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPsychicLinkComponent, MCXenoPsychicLinkActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoPsychicLinkComponent, MCXenoPsychicLinkDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<MCXenoPsychicLinkComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MCXenoPsychicLinkComponent, MobStateChangedEvent>(OnOwnerState);
        SubscribeLocalEvent<MCXenoPsychicLinkTargetComponent, MobStateChangedEvent>(OnTargetState);
        SubscribeLocalEvent<MCXenoPsychicLinkTargetComponent, DamageModifyAfterResistEvent>(OnDamage, before: [typeof(MCXenoOverhealSystem)]);
    }

    private bool CanLink(Entity<MCXenoPsychicLinkComponent> ent, EntityUid target) =>
        target != ent.Owner && IsLivingHiveMember(ent, target) && !_heal.CheckHealthThreshold(ent, ent.Comp.MinimumHealth)
        && ent.Comp.Target is null && !HasComp<MCXenoPsychicLinkTargetComponent>(ent)
        && !HasComp<MCXenoPsychicLinkTargetComponent>(target)
        && CompOrNull<MCXenoPsychicLinkComponent>(target)?.Target is null;

    private void OnAction(Entity<MCXenoPsychicLinkComponent> ent, ref MCXenoPsychicLinkActionEvent args)
    {
        if (args.Handled || !CanUseAction(ent, args.Action))
            return;

        var unlink = ent.Comp.Target == args.Target;
        if (!unlink && !CanLink(ent, args.Target))
            return;

        var ev = new MCXenoPsychicLinkDoAfterEvent(args.Action, EntityManager, unlink);
        var doAfter = new DoAfterArgs(EntityManager, ent, unlink ? ent.Comp.UnlinkDelay : ent.Comp.LinkDelay, ev, ent, args.Target)
        {
            DistanceThreshold = ent.Comp.Range,
            BreakOnMove = unlink,
            DuplicateCondition = DuplicateConditions.None,
            RequireCanInteract = false,
        };

        args.Handled = _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfter(Entity<MCXenoPsychicLinkComponent> ent, ref MCXenoPsychicLinkDoAfterEvent args)
    {
        if (args.Cancelled || !_mobState.IsAlive(ent) || args.Target is not { } target)
            return;

        if (args.Unlink)
        {
            if (ent.Comp.Target == target)
                Unlink(ent);

            return;
        }

        if (!CanLink(ent, target))
            return;

        _audio.PlayPredicted(ent.Comp.LinkSound, target, ent);
        _popup.PopupEntity(Loc.GetString("mc-xeno-gorger-link-start", ("gorger", ent.Owner)), target, target, PopupType.MediumXeno);

        ent.Comp.Target = target;
        Dirty(ent);

        var recipient = EnsureComp<MCXenoPsychicLinkTargetComponent>(target);
        recipient.Protector = ent;
        Dirty(target, recipient);

        SetLinkAura(ent, target, _transform.InRange(ent.Owner, target, ent.Comp.Range));

        ActionSetToggled<MCXenoPsychicLinkActionEvent>(ent, true);
    }

    private void Unlink(Entity<MCXenoPsychicLinkComponent> ent)
    {
        if (ent.Comp.Target is not { } target)
            return;

        SetLinkAura(ent, target, false);

        ent.Comp.Target = null;

        if (TryComp<MCXenoPsychicLinkTargetComponent>(target, out var recipient) && recipient.Protector == ent.Owner)
        {
            RemCompDeferred<MCXenoPsychicLinkTargetComponent>(target);
            _popup.PopupEntity(Loc.GetString("mc-xeno-gorger-link-end"), target, target);
        }

        if (TerminatingOrDeleted(ent))
            return;

        Dirty(ent);

        ActionSetToggled<MCXenoPsychicLinkActionEvent>(ent, false);
        ActionSetCooldown<MCXenoPsychicLinkActionEvent>(ent, ent.Comp.Cooldown);
    }

    private void OnDamage(Entity<MCXenoPsychicLinkTargetComponent> ent, ref DamageModifyAfterResistEvent args)
    {
        if (!TryComp<MCXenoPsychicLinkComponent>(ent.Comp.Protector, out var link) || link.Target != ent.Owner)
            return;

        var owner = ent.Comp.Protector;
        if (!IsLivingHiveMember(owner, ent) || IsDead(owner)
            || !_transform.InRange(owner, ent.Owner, link.Range))
            return;

        var margin = _heal.GetHealth(owner) - _heal.GetHealthAlive(owner) * link.MinimumHealth;
        if (margin <= 0)
            return;

        var remaining = new DamageSpecifier(args.Damage);
        var redirected = new DamageSpecifier();
        foreach (var type in MCXenoOverhealSystem.PhysicalDamageTypes)
        {
            if (margin <= 0 || !remaining.DamageDict.TryGetValue(type, out var amount) || amount <= 0)
                continue;

            var share = FixedPoint2.Min(amount * link.RedirectFraction, margin);
            margin -= share.Float();
            redirected.DamageDict[type] = share;
            remaining.DamageDict[type] -= share;
        }

        if (redirected.Empty)
            return;

        args.Damage = remaining;

        if (Net.IsServer)
            _damage.TryChangeDamage(owner, redirected, ignoreResistances: true, origin: args.Origin, tool: args.Tool);
    }

    private void SetLinkAura(Entity<MCXenoPsychicLinkComponent> owner, EntityUid target, bool inRange)
    {
        if (!inRange)
        {
            _aura.Remove(owner, AuraLayer);
            _aura.Remove(target, AuraLayer, owner.Owner);
            return;
        }

        _aura.Set(owner, AuraLayer, owner.Comp.Aura);
        _aura.Set(target, AuraLayer, owner.Comp.Aura, emitter: owner.Owner);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (Net.IsClient)
            return;

        var query = EntityQueryEnumerator<MCXenoPsychicLinkComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Target is not { } target)
                continue;

            if (!IsLivingHiveMember(uid, target) || _heal.CheckHealthThreshold(uid, comp.MinimumHealth))
            {
                Unlink((uid, comp));
                continue;
            }

            SetLinkAura((uid, comp), target, _transform.InRange(uid, target, comp.Range));
        }
    }

    private void OnOwnerState(Entity<MCXenoPsychicLinkComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive)
            Unlink(ent);
    }

    private void OnTargetState(Entity<MCXenoPsychicLinkTargetComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead && TryComp<MCXenoPsychicLinkComponent>(ent.Comp.Protector, out var link))
            Unlink((ent.Comp.Protector, link));
    }

    private void OnShutdown(Entity<MCXenoPsychicLinkComponent> ent, ref ComponentShutdown args) =>
        Unlink(ent);
}
