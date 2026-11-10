using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Events;
using Content.Shared._RMC14.Xenonids.Announce;
using Content.Shared.Mobs;

namespace Content.Shared._MC.Xeno.Hive.Systems.Main;

public abstract partial class MCSharedXenoHiveSystem
{
    [Dependency] private readonly SharedXenoAnnounceSystem _rmcXenoAnnounce = null!;

    private void InitializeRuler()
    {
        SubscribeLocalEvent<MCXenoHiveLeaderComponent, MCXenoHiveChangedEvent>(OnRulerHiveChanged);
        SubscribeLocalEvent<MCXenoHiveLeaderComponent, ComponentShutdown>(OnRulerShutdown);
        SubscribeLocalEvent<MCXenoHiveLeaderComponent, MobStateChangedEvent>(OnRulerMobStateChanged);
    }

    private void OnRulerHiveChanged(Entity<MCXenoHiveLeaderComponent> entity, ref MCXenoHiveChangedEvent args)
    {
        if (args.OldHive is not null)
            RemoveRuler(args.OldHive.Value, entity);

        if (args.Hive is not null)
            AddRuler(args.Hive.Value.Owner, entity.Owner);
    }

    private void OnRulerShutdown(Entity<MCXenoHiveLeaderComponent> entity, ref ComponentShutdown args)
    {
        MemberRemoveRuler(entity.Owner);
    }

    private void OnRulerMobStateChanged(Entity<MCXenoHiveLeaderComponent> entity, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
        {
            MemberRemoveRuler(entity.Owner);
            return;
        }

        if (args.OldMobState != MobState.Dead)
            return;

        MemberAddRuler(entity.Owner);
    }
}
