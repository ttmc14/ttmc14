using Content.Shared._MC.Xeno.Abilities.General.Skin.UI;
using Content.Shared._MC.Xeno.Blessings;
using Content.Shared._MC.Xeno.Construction.Blessings.UI;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Status.Events;
using Content.Shared._MC.Xeno.Hive.Status.UI;
using Content.Shared._MC.Xeno.Hive.Status.UI.Messages;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared._RMC14.TacticalMap;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Evolution;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared._RMC14.Xenonids.Watch;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Hive.Status;

public sealed class MCXenoHiveStatusSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly SharedUserInterfaceSystem _ui = null!;
    [Dependency] private readonly SharedXenoWatchSystem _rmcWatch = null!;
    [Dependency] private readonly MCSharedXenoHiveSystem _hive = null!;
    [Dependency] private readonly MCXenoHealSystem _heal = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _plasma = null!;

    private readonly Dictionary<EntityUid, float> _lastUpdateTimes = new();

    private const float UpdateInterval = 0.5f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenoComponent, MCXenoHiveStatusActionEvent>(OnAction);
        SubscribeLocalEvent<XenoComponent, MCXenoHiveStatusAlertEvent>(OnAlert);

        Subs.BuiEvents<XenoComponent>(MCXenoHiveStatusUI.Key,
            sub =>
        {
            sub.Event<MCXenoHiveStatusBlessingsMessage>(OnMessageBlessings);
            sub.Event<MCXenoHiveStatusDevolveMessage>(OnMessageDevolve);
            sub.Event<MCXenoHiveStatusEvolutionMessage>(OnMessageEvolution);
            sub.Event<MCXenoHiveStatusSkinMessage>(OnMessageSkin);
            sub.Event<MCXenoHiveStatusWatchMessage>(OnMessageWatch);
        });
    }

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var currentTime = (float) _timing.CurTime.TotalSeconds;
        var query = EntityQueryEnumerator<XenoComponent, UserInterfaceComponent>();

        while (query.MoveNext(out var uid, out _, out var uiComp))
        {
            var uiActors = uiComp.Actors;
            if (!uiActors.TryGetValue(MCXenoHiveStatusUI.Key, out var actors) || actors.Count == 0)
                continue;

            if (_lastUpdateTimes.TryGetValue(uid, out var lastTime) && lastTime + UpdateInterval > currentTime)
                continue;

            _lastUpdateTimes[uid] = currentTime;
            RefreshUI(uid);
        }
    }

    private void OnMessageWatch(Entity<XenoComponent> entity, ref MCXenoHiveStatusWatchMessage args) =>
        _rmcWatch.Watch(entity.Owner, GetEntity(args.TagetEntity));

    private void OnMessageSkin(Entity<XenoComponent> entity, ref MCXenoHiveStatusSkinMessage args) =>
        _ui.TryOpenUi(entity.Owner, MCXenoSkinUI.Key, entity);

    private void OnMessageEvolution(Entity<XenoComponent> entity, ref MCXenoHiveStatusEvolutionMessage args) =>
        _ui.TryOpenUi(entity.Owner, XenoEvolutionUIKey.Key, entity);

    private void OnMessageDevolve(Entity<XenoComponent> entity, ref MCXenoHiveStatusDevolveMessage args) =>
        _ui.TryOpenUi(entity.Owner, XenoDevolveUIKey.Key, entity);

    private void OnMessageBlessings(Entity<XenoComponent> entity, ref MCXenoHiveStatusBlessingsMessage args) =>
        _ui.TryOpenUi(entity.Owner, MCXenoBlessingsUiKey.Key, entity);

    private void OnAction(Entity<XenoComponent> entity, ref MCXenoHiveStatusActionEvent args)
    {
        args.Handled = true;
        OpenUI(entity);
    }

    private void OnAlert(Entity<XenoComponent> entity, ref MCXenoHiveStatusAlertEvent args)
    {
        args.Handled = true;
        OpenUI(entity);
    }

    private void OpenUI(Entity<XenoComponent> entity)
    {
        _ui.TryOpenUi(entity.Owner, MCXenoHiveStatusUI.Key, entity);
        RefreshUI(entity.Owner);
    }

    private void RefreshUI(EntityUid uid)
    {
        if (GenerateXenosState(uid) is not { } state)
            return;

        _ui.SetUiState(uid, MCXenoHiveStatusUI.Key, state);
    }

    private MCXenoHiveStatusXenosBuiState? GenerateXenosState(EntityUid uid)
    {
        if (!_hive.TryGetHive(uid, out var hive))
            return null;

        var evolution = 0;
        var evolutionMax = 0;
        var tierSlots = _hive.GetTierSlots((hive, hive));

        tierSlots[-1] = -1;
        tierSlots[0] = -1;
        tierSlots[1] = -1;
        tierSlots[4] = -1;

        var devolveHide = true;
        var evolutionHide = true;
        var evolutionPointsHide = true;

        if (TryComp<XenoDevolveComponent>(uid, out var devolveComponent) && devolveComponent.DevolvesTo.Length > 0)
            devolveHide = false;

        if (TryComp<XenoEvolutionComponent>(uid, out var evolutionComponent))
        {
            evolutionHide = false;
            if (evolutionComponent.EvolvesTo.Count > 0)
                evolutionPointsHide = false;

            // What? I fuck this shit: RA0002: Tried to perform an 'Execute' other-type access to member 'Points' in type 'XenoEvolutionComponent', despite only having 'Read' access. Type Permissions: rwxrwxr--.
            // Bro it's node execute IT IS READ
            var evoPointsRaw = evolutionComponent.Points;
            var evoMaxRaw = evolutionComponent.Max;

            evolution = evoPointsRaw.Int();
            evolutionMax = evoMaxRaw.Int();
        }

        return new MCXenoHiveStatusXenosBuiState
        {
            Psypoints = hive.Comp.Psypoints,
            LarvaPoints = hive.Comp.LarvaPoints,
            BurrowedLarva = _hive.GetBurrowedLarva((hive, hive)),
            TierSlots = tierSlots,
            BlessingsHide = !HasComp<MCXenoBlessingsComponent>(uid),
            DevolutionHide = devolveHide,
            EvolutionHide = evolutionHide,
            EvolutionPoints = evolution,
            EvolutionPointsMax = evolutionMax,
            EvolutionPointsHide = evolutionPointsHide,
            Xenos = GenerateXenosEntities(hive),
            Structures = GenerateStructureEntities(hive),
        };
    }

    private List<MCXenoEntity> GenerateXenosEntities(Entity<MCXenoHiveComponent> hive)
    {
        var list = new List<MCXenoEntity>();
        var query = EntityQueryEnumerator<XenoComponent, MobStateComponent, HiveMemberComponent>();

        while (query.MoveNext(out var uid, out var xenoComponent, out var mobComponent, out var memberComponent))
        {
            if (mobComponent.CurrentState == MobState.Dead || !_hive.IsMember((uid, memberComponent), hive.Owner))
                continue;

            var meta = MetaData(uid);
            list.Add(new MCXenoEntity(
                GetNetEntity(uid),
                meta.EntityName,
                xenoComponent.Tier,
                _heal.GetHealthStateRatio(uid),
                _plasma.GetPlasmaNormalized(uid),
                meta.EntityPrototype?.ID));
        }
        return list;
    }

    private List<MCXenoStructure> GenerateStructureEntities(Entity<MCXenoHiveComponent> hive)
    {
        var list = new List<MCXenoStructure>();
        var query = EntityQueryEnumerator<XenoStructureMapTrackedComponent, HiveMemberComponent>();

        while (query.MoveNext(out var uid, out _, out var memberComponent))
        {
            if (!_hive.IsMember((uid, memberComponent), hive.Owner))
                continue;

            var meta = MetaData(uid);
            list.Add(new MCXenoStructure(GetNetEntity(uid), meta.EntityName, 1f, meta.EntityPrototype?.ID));
        }
        return list;
    }
}
