using Content.Server._MC.Rules.Common;
using Content.Server._MC.Xeno.Hive;
using Content.Shared._MC;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Roles.Jobs;
using Content.Shared._RMC14.Thunderdome;
using Robust.Shared.Player;
using Content.Shared._RMC14.Marines;
using Robust.Shared.Timing;

namespace Content.Server._MC.Xeno;

public sealed class MCXenoAutoSpawnBalanceSystem : MCEntitySystemSingleton<MCXenoAutoSpawnBalanceSingletonComponent>
{
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly MCRuleXenoMarinesSpawnSystem _mcRuleXenoMarinesSpawn = null!;
    [Dependency] private readonly MCXenoHiveSystem _hive = null!;
    [Dependency] private readonly SharedMindSystem _mind = null!;
    [Dependency] private readonly SharedJobSystem _jobs = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!Inst.Comp.Enabled || Inst.Comp.NextUpdate > _timing.CurTime)
            return;

        Inst.Comp.NextUpdate = Inst.Comp.UpdateInterval + _timing.CurTime;

        OnUpdate();
    }

    public void Enable()
    {
        if (Inst.Comp.Enabled)
            return;

        Inst.Comp.Enabled = true;
        Inst.Comp.NextUpdate = _timing.CurTime + Inst.Comp.UpdateInterval;
    }

    public void Disable()
    {
        if (!Inst.Comp.Enabled)
            return;

        Inst.Comp.Enabled = false;
    }

    private void OnUpdate()
    {
        if (!_mcRuleXenoMarinesSpawn.Spawned)
            return;

        if (_hive.DefaultHive is not { } hive)
            return;

        var xenos = GetTotalXenos(hive);
        var deficit = (int) (GetMarinePoints() / Inst.Comp.XenoWeight) - xenos;
        if (deficit > 0)
        {
            _hive.AddBurrowedLarva(hive, deficit);
            return;
        }

        if (xenos == 0)
        {
            _hive.AddBurrowedLarva(hive, 2);
        }
    }

    private float GetMarinePoints()
    {
        var points = 0f;
        var marines = EntityQueryEnumerator<MarineComponent, ActorComponent>();
        while (marines.MoveNext(out var uid, out _, out _))
        {
            if (_mobState.IsDead(uid) || HasComp<ThunderdomeMapComponent>(Transform(uid).MapUid))
                continue;

            if (!_mind.TryGetMind(uid, out var mind, out _) || !_jobs.MindTryGetJob(mind, out var job))
                continue;

            points += job.ID switch
            {
                "MCFC" => 6f,
                "MCCMO" or "MCDoctor" or "MCResearcher" => 1f,
                "MCTransportOfficer" => 1.5f,
                _ => MCSharedXenoHiveSystem.LarvaPointsRegular,
            };
        }

        return points;
    }

    private int GetTotalXenos(EntityUid hive)
    {
        return _hive.GetLivingXenos(hive, Inst.Comp.XenoMinTier) + _hive.GetBurrowedLarva(hive);
    }
}

[RegisterComponent]
public sealed partial class MCXenoAutoSpawnBalanceSingletonComponent : Component
{
    [DataField]
    public bool Enabled;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan NextUpdate;

    [DataField]
    public float XenoWeight = 9f;

    [DataField]
    public int XenoMinTier;
}
