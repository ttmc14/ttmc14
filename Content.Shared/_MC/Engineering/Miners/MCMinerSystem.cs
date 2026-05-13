using Content.Shared._MC.ASRS.Systems;
using Content.Shared._MC.Chat;
using Content.Shared._MC.Damage;
using Content.Shared._MC.Damage.Integrity.Events;
using Content.Shared._MC.Damage.Integrity.Systems;
using Content.Shared._MC.Engineering.Miners.Components;
using Content.Shared._MC.Engineering.Miners.Events;
using Content.Shared._RMC14.Marines.Skills;
using Content.Shared._RMC14.TacticalMap;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tools.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Engineering.Miners;

public sealed class MCMinerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly SharedToolSystem _tool = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;

    [Dependency] private readonly SkillsSystem _rmcSkills = null!;

    [Dependency] private readonly MCASRSSystem _mcAsrs = null!;
    [Dependency] private readonly MCIntegritySystem _mcIntegrity = null!;
    [Dependency] private readonly MCDamageableSystem _mcDamageable = null!;
    [Dependency] private readonly MCSharedChatSystem _mcChat = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCMinerComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<MCMinerComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<MCMinerComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<MCMinerComponent, MCMinerRepairDoAfterEvent>(OnRepairDoAfter);
        SubscribeLocalEvent<MCMinerComponent, MCIntegrityTriggeredEvent>(OnIntegrityTriggered);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCMinerComponent>();
        while (query.MoveNext(out _, out var minerComponent))
        {
            if (minerComponent.State != MCMinerState.Running)
                continue;

            if (minerComponent.MineralStored >= minerComponent.MineralStorage)
                continue;

            if (minerComponent.NextMineralProduction > _timing.CurTime)
                continue;

            minerComponent.MineralStored++;
            minerComponent.NextMineralProduction = _timing.CurTime + minerComponent.MineralProductionTime;
        }
    }

    private void OnExamined(Entity<MCMinerComponent> entity, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(MCMinerComponent)))
        {
            args.PushMarkup(Loc.GetString("mc-miner-examine-storage", ("count", entity.Comp.MineralStored)));

            if (entity.Comp.State == MCMinerState.Running)
                return;

            var state = entity.Comp.State;
            var repairMessage = state switch
            {
                MCMinerState.Destroyed => "mc-miner-examine-repair-destroyed",
                MCMinerState.MediumDamage => "mc-miner-examine-repair-medium",
                MCMinerState.SmallDamage => "mc-miner-examine-repair-small",
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
            };


            args.PushMarkup(Loc.GetString(repairMessage, ("miner", entity)));
        }
    }

    private void OnInteractHand(Entity<MCMinerComponent> entity, ref InteractHandEvent args)
    {
        args.Handled = true;

        if (entity.Comp.State != MCMinerState.Running)
            return;

        if (entity.Comp.MineralStored == 0)
        {
            _mcChat.TrySendInGameICSpeakMessage(entity, Loc.GetString("mc-miner-not-ready", ("miner", entity)));
            return;
        }

        var value = entity.Comp.MineralStored * entity.Comp.MineralValue;

        _mcAsrs.AddBalance(value);
        _mcChat.TrySendInGameICSpeakMessage(entity, Loc.GetString("mc-miner-sold", ("value", value)));

        entity.Comp.MineralStored = 0;
    }

    private void OnInteractUsing(Entity<MCMinerComponent> entity, ref InteractUsingEvent args)
    {
        var user = args.User;
        var used = args.Used;

        args.Handled = true;

        // Step 1.
        if (_tool.HasQuality(used, entity.Comp.WeldingQuality))
        {
            TryRepair(entity, user, used, MCMinerState.Destroyed);
            return;
        }

        // Step 2.
        if (_tool.HasQuality(used, entity.Comp.CuttingQuality))
        {
            TryRepair(entity, user, used, MCMinerState.MediumDamage);
            return;
        }

        // Step 3.
        if (_tool.HasQuality(used, entity.Comp.WrenchQuality))
            TryRepair(entity, user, used, MCMinerState.SmallDamage);
    }

    private void OnRepairDoAfter(Entity<MCMinerComponent> entity, ref MCMinerRepairDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        if (entity.Comp.State != args.State)
            return;

        entity.Comp.State = args.State switch
        {
            MCMinerState.Destroyed => MCMinerState.MediumDamage,
            MCMinerState.MediumDamage => MCMinerState.SmallDamage,
            MCMinerState.SmallDamage => MCMinerState.Running,
            _ => throw new ArgumentOutOfRangeException(),
        };

        switch (entity.Comp.State)
        {
            case MCMinerState.Running:
                _mcIntegrity.ResetIntegrity(entity.Owner);

                entity.Comp.NextMineralProduction = _timing.CurTime + entity.Comp.MineralProductionTime;
                break;

            case MCMinerState.Destroyed:
            case MCMinerState.SmallDamage:
            case MCMinerState.MediumDamage:
                var integrityId = entity.Comp.State switch
                {
                    MCMinerState.SmallDamage => "MinerSmallDamaged",
                    MCMinerState.MediumDamage => "MinerMediumDamaged",
                    MCMinerState.Destroyed => "MinerDestroyed",
                    _ => throw new ArgumentOutOfRangeException()
                };

                var integrity = _mcIntegrity.GetIntegrity(entity.Owner, integrityId);
                var damage = _mcIntegrity.GetTotalDamage(entity.Owner);

                if (damage > integrity)
                    _mcIntegrity.SetIntegrity(entity.Owner, integrityId, _mcDamageable.DamageBrute);

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        Dirty(entity);

        UpdateIcon(entity);
        UpdateAppearance(entity);
    }

    private void OnIntegrityTriggered(Entity<MCMinerComponent> entity, ref MCIntegrityTriggeredEvent args)
    {
        entity.Comp.State = args.IntegrityId.Id switch
        {
            "MinerSmallDamaged" => MCMinerState.SmallDamage,
            "MinerMediumDamaged" => MCMinerState.MediumDamage,
            "MinerDestroyed" => MCMinerState.Destroyed,
            _ => throw new ArgumentOutOfRangeException(),
        };

        UpdateIcon(entity);
        UpdateAppearance(entity);
    }

    private void UpdateAppearance(Entity<MCMinerComponent> entity)
    {
        switch (entity.Comp.State)
        {
            case MCMinerState.Running:
                _appearance.SetData(entity, MCMinerLayers.Layer, MCMinerState.Running);
                return;

            case MCMinerState.Destroyed:
                _appearance.SetData(entity, MCMinerLayers.Layer, MCMinerState.Destroyed);
                return;

            case MCMinerState.MediumDamage:
                _appearance.SetData(entity, MCMinerLayers.Layer, MCMinerState.MediumDamage);
                return;

            case MCMinerState.SmallDamage:
                _appearance.SetData(entity, MCMinerLayers.Layer, MCMinerState.SmallDamage);
                return;
        }
    }

    private void UpdateIcon(Entity<MCMinerComponent> entity)
    {
        if (!TryComp<TacticalMapIconComponent>(entity, out var iconComponent) || iconComponent.Icon is not { } icon)
            return;

        var ensure = EnsureComp<MapBlipIconOverrideComponent>(entity);
        var state = entity.Comp.State == MCMinerState.Running ? "phoron-on" : "phoron";
        var newIcon = new SpriteSpecifier.Rsi(icon.RsiPath, state);

        ensure.Icon = newIcon;
        Dirty(entity, ensure);
    }

    private void TryRepair(Entity<MCMinerComponent> entity, EntityUid user, EntityUid used, MCMinerState state)
    {
        if (entity.Comp.State == MCMinerState.Running)
        {
            _popup.PopupClient(Loc.GetString("mc-miner-repair-not-needed", ("miner", entity)), entity, user, PopupType.LargeCaution);
            return;
        }

        if (entity.Comp.State != state)
        {
            _popup.PopupClient(Loc.GetString("mc-miner-repair-different-tool", ("miner", entity)), entity, user, PopupType.LargeCaution);
            return;
        }

        var quality = state switch
        {
            MCMinerState.Destroyed => entity.Comp.WeldingQuality,
            MCMinerState.MediumDamage => entity.Comp.CuttingQuality,
            MCMinerState.SmallDamage => entity.Comp.WrenchQuality,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
        };

        var toolUsed = _tool.UseTool(
            used,
            user,
            entity,
            (float) GetRepairDelay(user).TotalSeconds,
            quality,
            new MCMinerRepairDoAfterEvent(state),
            entity.Comp.WeldingCost,
            duplicateCondition: DuplicateConditions.SameTool
        );

        if (!toolUsed)
            return;

        _popup.PopupClient(Loc.GetString("mc-miner-repair-start-self", ("miner", entity), ("tool", used)), entity, user);
    }

    private TimeSpan GetRepairDelay(EntityUid user)
    {
        return TimeSpan.FromSeconds(10) - TimeSpan.FromSeconds(2) * _rmcSkills.GetSkill(user, "MCSkillEngineer");
    }
}
