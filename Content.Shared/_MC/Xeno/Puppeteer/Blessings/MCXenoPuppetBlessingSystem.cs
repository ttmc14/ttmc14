using Content.Shared._MC.Xeno.Abilities.Puppeteer;
using Content.Shared._RMC14.Damage;
using Content.Shared._RMC14.Xenonids.Stab;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;

namespace Content.Shared._MC.Xeno.Abilities.Puppeteer.Blessings;

public sealed class MCXenoPuppetBlessingSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPuppeteerComponent, MCXenoBlessingSelectorActionEvent>(OnAction);

        Subs.BuiEvents<MCXenoPuppeteerComponent>(MCXenoPuppetBlessingSelectorUi.Key, subs =>
        {
            subs.Event<MCXenoPuppetBlessingChosenBuiMsg>(OnBlessingChosen);
        });
    }

    private void OnAction(Entity<MCXenoPuppeteerComponent> ent, ref MCXenoBlessingSelectorActionEvent args)
    {
        if (ent.Comp.SelectedPuppet == null)
        {
            _popup.PopupClient("first seleact a puppet", ent.Owner);
            return;
        }

        _ui.TryOpenUi(ent.Owner, MCXenoPuppetBlessingSelectorUi.Key, ent);
    }

    private void OnBlessingChosen(Entity<MCXenoPuppeteerComponent> ent, ref MCXenoPuppetBlessingChosenBuiMsg args)
    {
        if (_net.IsClient)
            return;

        if (ent.Comp.SelectedPuppet == null)
        {
            _popup.PopupClient("first seleact a puppet", ent.Owner);
            return;
        }

        switch (args.Blessing)
        {
            case MCXenoPuppetBlessing.Warding:
                EnsureComp<MCXenoPuppetBlessingWardingComponent>(ent.Comp.SelectedPuppet.Value);
                break;

            case MCXenoPuppetBlessing.Fury:
                EnsureComp<MCXenoPuppetBlessingFuryComponent>(ent.Comp.SelectedPuppet.Value);
                break;

            case MCXenoPuppetBlessing.Frenzy:
                EnsureComp<MCXenoPuppetBlessingFrenzyComponent>(ent.Comp.SelectedPuppet.Value);
                break;
        }

    }
}