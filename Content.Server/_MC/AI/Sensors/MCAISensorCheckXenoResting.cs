using Content.Shared._MC.AI.Modules;
using Content.Shared._RMC14.Xenonids.Rest;

namespace Content.Server._MC.AI.Sensors;

public sealed partial class MCAISensorCheckXenoResting : MCAISensor<MCAISensorCheckXenoResting>
{
    public override TimeSpan UpdateInterval => TimeSpan.FromSeconds(0.1f);
}

public sealed class MCAISensorCheckXenoRestingSystem : MCAISensorHasComponentSystem<MCAISensorCheckXenoResting, XenoRestingComponent>;
