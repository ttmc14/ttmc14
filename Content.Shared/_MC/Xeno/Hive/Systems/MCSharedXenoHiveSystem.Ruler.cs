using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Events;
using Content.Shared._RMC14.Xenonids.Announce;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Mobs;

namespace Content.Shared._MC.Xeno.Hive.Systems;

public abstract partial class MCSharedXenoHiveSystem
{
    [Dependency] private readonly SharedXenoAnnounceSystem _rmcXenoAnnounce = null!;

    private void InitializeRuler()
    {
        SubscribeLocalEvent<MCXenoHiveLeaderComponent, HiveChangedEvent>(OnRulerHiveChanged);
        SubscribeLocalEvent<MCXenoHiveLeaderComponent, ComponentShutdown>(OnRulerShutdown);
        SubscribeLocalEvent<MCXenoHiveLeaderComponent, MobStateChangedEvent>(OnRulerMobStateChanged);
    }

    private void OnRulerHiveChanged(Entity<MCXenoHiveLeaderComponent> entity, ref HiveChangedEvent args)
    {
        if (args.OldHive is not null)
            HiveRemoveRuler(args.OldHive.Value, entity);

        if (args.Hive is not null)
            HiveAddRuler((args.Hive.Value.Owner, args.Hive.Value.Comp), entity.Owner);
    }

    private void OnRulerShutdown(Entity<MCXenoHiveLeaderComponent> entity, ref ComponentShutdown args)
    {
        HiveMemberRemoveRuler(entity.Owner);
    }

    private void OnRulerMobStateChanged(Entity<MCXenoHiveLeaderComponent> entity, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
        {
            HiveMemberRemoveRuler(entity.Owner);
            return;
        }

        if (args.OldMobState != MobState.Dead)
            return;

        HiveMemberAddRuler(entity.Owner);
    }

    public bool HiveAddRuler(Entity<HiveComponent?> entity, EntityUid rulerUid)
    {
        if (!Resolve(entity, ref entity.Comp))
            return false;

        if (entity.Comp.Rulers.Contains(rulerUid))
            return false;

        entity.Comp.Rulers.Add(rulerUid);
        Dirty(entity);

        var ev = new MCXenoHiveRulerAdded(rulerUid, entity.Owner);
        RaiseLocalEvent(entity, ref ev);

        _rmcXenoAnnounce.AnnounceSameHive(rulerUid, Loc.GetString("mc-xeno-hive-ruler-new"));
        return true;
    }

    public bool HiveRemoveRuler(Entity<HiveComponent?> entity, EntityUid rulerUid)
    {
        if (!Resolve(entity, ref entity.Comp))
            return false;

        var result = entity.Comp.Rulers.Remove(rulerUid);
        Dirty(entity);

        // _rmcXenoAnnounce.AnnounceSameHive(entity.Owner, Loc.GetString("mc-xeno-hive-ruler-dead"));

        return result;
    }

    public bool HiveMemberAddRuler(EntityUid rulerUid)
    {
        if (!_hiveMemberQuery.TryComp(rulerUid, out var hiveMemberComponent) || !hiveMemberComponent.Hive.HasValue)
            return false;

        return HiveAddRuler(hiveMemberComponent.Hive.Value, rulerUid);
    }

    public bool HiveMemberRemoveRuler(EntityUid rulerUid)
    {
        if (!_hiveMemberQuery.TryComp(rulerUid, out var hiveMemberComponent) || !hiveMemberComponent.Hive.HasValue)
            return false;

        return HiveRemoveRuler(hiveMemberComponent.Hive.Value, rulerUid);
    }

    public bool HiveHasRuler(Entity<HiveComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp))
            return false;

        return entity.Comp.Rulers.Count > 0;
    }

    public bool HiveMemberHasRuler(EntityUid entity)
    {
        if (!_hiveMemberQuery.TryComp(entity, out var hiveMemberComponent))
            return false;

        return hiveMemberComponent.Hive is not null && HiveHasRuler(hiveMemberComponent.Hive.Value);
    }
}
