using System.Numerics;
using Content.Shared._MC.AI;
using Content.Shared._MC.AI.Events;
using Content.Shared._MC.AI.Modules;
using Content.Shared.Examine;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Server._MC.AI.Sensors;

public sealed partial class MCAISensorNearestGun : MCAISensorNearestComponent<MCAISensorNearestGun>
{
    public override TimeSpan UpdateInterval => TimeSpan.FromSeconds(0.5f);
}

public sealed partial class MCAISensorNearestGunSystem : MCAISensorNearestWithComponentSystem<MCAISensorNearestGun, GunComponent>;
