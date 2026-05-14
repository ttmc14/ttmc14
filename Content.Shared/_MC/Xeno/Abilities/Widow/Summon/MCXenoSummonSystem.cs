using System.Linq;
using Content.Shared._MC.Xeno.Abilities.Widow.Order;
using Content.Shared._MC.Xeno.Abilities.Widow.Order.Events;
using Content.Shared.Mobs;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Widow.Summon;

public sealed partial class MCXenoSummonSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly MCXenoOrderSystem _mcXenoOrder = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoSummonComponent, MCXenoSummonActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoSummonComponent, MCXenoOrderRaise>(OnOrder);

        SubscribeLocalEvent<MCXenoSummonedComponent, MobStateChangedEvent>(OnSummonedMobStateChanged);
        SubscribeLocalEvent<MCXenoSummonedComponent, ComponentShutdown>(OnSummonedShutdown);
        SubscribeLocalEvent<MCXenoSummonedComponent, PreventCollideEvent>(OnSummonedPreventCollide);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoSummonedComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            var selfPosition = _transform.GetWorldPosition(uid);
            var ownerPosition = _transform.GetWorldPosition(component.OwnerUid);
            var delta = selfPosition - ownerPosition;

            if (delta.Length() <= component.OutDamageRange && component.OwnerUid.Valid)
                continue;

            if (component.OutDamageNext > _timing.CurTime)
                continue;

            component.OutDamageNext = component.OutDamageDelay + _timing.CurTime;
            Dirty(uid, component);
        }
    }

    private void OnAction(Entity<MCXenoSummonComponent> entity, ref MCXenoSummonActionEvent args)
    {
        if (args.Handled)
            return;

        if (entity.Comp.SummonUids.Count >= entity.Comp.Limit)
            return;

        if (!TryUseAction(entity, args.Action))
            return;

        Spawn(entity);

        args.Handled = true;
    }

    private void OnOrder(Entity<MCXenoSummonComponent> entity, ref MCXenoOrderRaise args)
    {
        foreach (var summonUid in entity.Comp.SummonUids.Where(summonUid => summonUid.Valid))
        {
            _mcXenoOrder.SetOrder(entity, summonUid, args.OrderId);
        }
    }

    private void Spawn(Entity<MCXenoSummonComponent> entity)
    {
        var coordinates = Transform(entity).Coordinates;
        var instance = PredictedSpawnAtPosition(entity.Comp.ProtoId, coordinates);

        MCXenoHive.SetSameHive(entity.Owner, instance);

        var summonedComponent = EnsureComp<MCXenoSummonedComponent>(instance);
        summonedComponent.OwnerUid = entity.Owner;

        entity.Comp.SummonUids.Add(instance);

        _mcXenoOrder.ReplyOrder(entity, instance);

        Dirty(instance, summonedComponent);
        Dirty(entity);
    }
}
