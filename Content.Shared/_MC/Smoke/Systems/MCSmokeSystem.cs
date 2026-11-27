using Content.Shared._MC.Smoke.Components;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._MC.Damage;
using Content.Shared._RMC14.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Smoke.Systems;

public sealed partial class MCSmokeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly RMCMapSystem _rmcMap = null!;
    [Dependency] private readonly MCDamageableSystem _mcDamageable = null!;

    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;

    [ViewVariables] private readonly Dictionary<EntityUid, TimeSpan> _immunity = new();
    [ViewVariables] private readonly List<EntityUid> _immunityToRemove = new();
    [ViewVariables] private readonly HashSet<EntityUid> _toProcess = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCSmokeComponent, StartCollideEvent>(OnCollideStart);
        SubscribeLocalEvent<MCSmokeComponent, EndCollideEvent>(OnCollideEnd);
    }

    private void OnCollideStart(Entity<MCSmokeComponent> entity, ref StartCollideEvent args)
    {
        if (TerminatingOrDeleted(args.OtherEntity))
            return;

        entity.Comp.AffectedEntities.Add(args.OtherEntity);
    }

    private void OnCollideEnd(Entity<MCSmokeComponent> entity, ref EndCollideEvent args)
    {
        entity.Comp.AffectedEntities.Remove(args.OtherEntity);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCSmokeComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.EffectNext > _timing.CurTime)
                continue;

            component.EffectNext = _timing.CurTime + component.EffectDelay;

            Process((uid, component));
        }

        _immunityToRemove.Clear();

        foreach (var (affectedUid, duration) in _immunity)
        {
            if (duration > _timing.CurTime)
                continue;

            _immunityToRemove.Add(affectedUid);
        }

        foreach (var affectedUid in _immunityToRemove)
        {
            _immunity.Remove(affectedUid);
        }
    }

    private void Process(Entity<MCSmokeComponent> entity)
    {
        _toProcess.Clear();
        entity.Comp.AffectedEntities.RemoveWhere(uid => TerminatingOrDeleted(uid));
        _toProcess.UnionWith(entity.Comp.AffectedEntities);

        var anchored = _rmcMap.GetAnchoredEntitiesEnumerator(entity.Owner);
        while (anchored.MoveNext(out var uid))
        {
            _toProcess.Add(uid);
        }

        foreach (var affectedUid in _toProcess)
        {
            Affect(entity, affectedUid);
        }
    }

    private void Affect(Entity<MCSmokeComponent> entity, EntityUid affectedUid)
    {
        if (TerminatingOrDeleted(affectedUid))
            return;

        if (HasComp<MCSmokeDamageComponent>(entity) && !_mcDamageable.CanAreaAttackTarget(entity, affectedUid))
            return;

        if (_immunity.TryGetValue(affectedUid, out var immunityTime) && immunityTime > _timing.CurTime)
            return;

        _immunity[affectedUid] = _timing.CurTime + entity.Comp.EffectDelay;

        var ev = new MCSmokeEffectEvent(affectedUid);
        RaiseLocalEvent(entity, ref ev);
    }
}
