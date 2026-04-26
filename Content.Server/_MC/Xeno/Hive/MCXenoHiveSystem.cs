using Content.Server._RMC14.Xenonids.Hive;
using Content.Server.GameTicking.Events;
using Content.Shared._MC.Xeno.Hive.Systems;
using Content.Shared._MC.Xeno.Hive.Systems.Main;

namespace Content.Server._MC.Xeno.Hive;

public sealed class MCXenoHiveSystem : MCSharedXenoHiveSystem
{
    [Dependency] private readonly XenoHiveSystem _rmcHive = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        Inst.Comp.DefaultHive = _rmcHive.CreateHive(Inst.Comp.DefaultHiveName, Inst.Comp.DefaultHiveId);
        Dirty();
    }
}
