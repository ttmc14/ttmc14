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

namespace Content.Shared._MC.Xeno.Abilities.Puppeteer;

public sealed class MCXenoPuppeteerSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<MCXenoPuppeteerComponent>(MCXenoPuppetSelectorUi.Key, subs =>
        {
            subs.Event<MCXenoPuppetChosenBuiMsg>(OnPuppetChosen);
        });
    }

    private void OnPuppetChosen(Entity<MCXenoPuppeteerComponent> ent, ref MCXenoPuppetChosenBuiMsg args)
    {
        if (_net.IsClient)
            return;

        ent.Comp.SelectedPuppet = GetEntity(args.Puppet);
        Dirty(ent);
    }
}