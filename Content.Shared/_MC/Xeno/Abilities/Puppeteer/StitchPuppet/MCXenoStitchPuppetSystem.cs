using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared._MC.Xeno.Plasma.Components.Conversions;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Emote;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.DoAfter;

namespace Content.Shared._MC.Xeno.Abilities.Puppeteer.StitchPuppet;

public sealed class MCXenoStitchPuppetSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedRMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly SharedRMCEmoteSystem _rmcEmoteSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoStitchPuppetComponent, MCXenoStitchPuppetActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoStitchPuppetComponent, MCXenoStitchPuppetDoAfterEvent>(OnStitchPuppetDoAfter);
    }

    private void OnAction(Entity<MCXenoStitchPuppetComponent> ent, ref MCXenoStitchPuppetActionEvent args)
    {
        if (args.Handled)
            return;

        if (!RMCActions.TryUseAction(ent, args.Action, ent))
            return;

        args.Handled = true;

        if (!TryComp<MCXenoPuppeteerComponent>(ent, out var puppeteerComp))
            return;

        if (puppeteerComp.Puppets.Count >= puppeteerComp.MaxPuppets)
        {
            _popup.PopupClient($"too many puppets (max: {puppeteerComp.MaxPuppets}", ent);
            return;
        }

        var doAfter = new DoAfterArgs(EntityManager, ent, ent.Comp.StitchDelay, new MCXenoStitchPuppetDoAfterEvent(), ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true
        };


        if (_doAfter.TryStartDoAfter(doAfter))
            _popup.PopupClient($"{ToPrettyString(ent.Owner)} begins to vomit out biomass and skillfully sews various bits and pieces together!", ent, ent, PopupType.MediumCaution);
    }

    private void OnStitchPuppetDoAfter(Entity<MCXenoStitchPuppetComponent> ent, ref MCXenoStitchPuppetDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (!_mcXenoPlasma.TryRemovePlasma(ent.Owner, ent.Comp.PlasmaCost))
            return;

        if (!TryComp<MCXenoPuppeteerComponent>(ent, out var puppeteerComp))
            return;

        if (puppeteerComp.Puppets.Count >= puppeteerComp.MaxPuppets)
        {
            _popup.PopupClient($"too many puppets (max: {puppeteerComp.MaxPuppets})", ent);
            return;
        }

        args.Handled = true;

        _popup.PopupClient($"{ToPrettyString(ent.Owner)} forms a repulsive puppet!", ent, ent, PopupType.MediumCaution);

        var xform = Transform(ent.Owner);
        var puppet = SpawnServer(ent.Comp.PuppetProto, xform.Coordinates);

        if (puppet != EntityUid.Invalid)
        {
            puppeteerComp.Puppets.Add(puppet);
            Dirty(ent);
        }
    }
}
