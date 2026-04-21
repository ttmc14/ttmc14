using Content.Shared._MC.AI.Modules;
using Content.Shared._RMC14.Entrenching;

namespace Content.Server._MC.AI.Sensors;

public sealed partial class MCAISensorNearestBarricade : MCAISensorNearestComponent<MCAISensorNearestBarricade>
{
    public override TimeSpan UpdateInterval => TimeSpan.FromSeconds(0.5f);
}

public sealed partial class MCAISensorNearestBarricadeSystem : MCAISensorNearestWithComponentSystem<MCAISensorNearestBarricade, BarricadeComponent>;
