using Content.Server.NPC.HTN;
using Content.Shared._MC.Engineering.Sentries;
using Content.Shared._MC.Engineering.Sentries.Components;

namespace Content.Server._MC.Engineering.Sentries;

public sealed class MCSentryRadiaSystem : MCSentryRadialSharedSystem
{
    protected override void SetRadius(Entity<MCSentryRadialComponent> entity, float radius)
    {
        if (!TryComp<HTNComponent>(entity, out var htnComponent))
            return;

        var blackboard = htnComponent.Blackboard;

        blackboard.SetValue("VisionRadius", radius);
        blackboard.SetValue("AggroVisionRadius", radius);
    }
}
