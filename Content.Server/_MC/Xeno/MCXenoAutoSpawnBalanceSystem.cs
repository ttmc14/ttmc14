using Content.Server._MC.Xeno.Hive;
using Content.Shared._MC;
using Content.Shared._MC.Living;
using Content.Shared._RMC14.Marines;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Server._MC.Xeno;

public sealed class MCXenoAutoSpawnBalanceSystem : MCEntitySystemSingleton<MCXenoAutoSpawnBalanceSingletonComponent>
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly MCXenoHiveSystem _hive = null!;
    [Dependency] private readonly MCLivingSystem _living = null!;

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
    }

    private void OnUpdate()
    {
        if (_hive.DefaultHive is not {} hive)
            return;

        var diff = GetPointDifference(hive);
        if (diff >= Inst.Comp.XenoWeight || GetTotalXenos(hive) < Inst.Comp.XenoMin)
            _hive.AddBurrowedLarva(hive, 1);
    }

    private float GetPointDifference(EntityUid hive)
    {
        var marines = _living.Get<MarineComponent>();
        return marines * Inst.Comp.MarineWeight - GetTotalXenos(hive) * Inst.Comp.XenoWeight;
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
    public float MarineWeight = 3.55f;

    [DataField]
    public float XenoWeight = 10f;

    [DataField]
    public int XenoMinTier;

    [DataField]
    public int XenoMin = 2;
}
