using Content.Shared._MC.Mob.Movement;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Panther.Adrenalin;

public sealed class MCXenoAdrenalinSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoAdrenalinComponent, MCMobStepEvent>(OnStep);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MCXenoAdrenalinComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.UpdateNext > _timing.CurTime)
                continue;

            component.UpdateNext = _timing.CurTime + component.UpdateDelay;
            Dirty(uid, component);

            var recentlyMoved = component.LastStep + component.GainDelay >= _timing.CurTime;
            if (recentlyMoved)
            {
                _mcXenoPlasma.RegenPlasma(uid, component.GainPlasma);
                continue;
            }

            if (_mcXenoPlasma.GetPlasma(uid) <= component.DrainPlasmaMin)
                continue;

            _mcXenoPlasma.RemovePlasma(uid, component.DrainPlasma);
        }
    }

    private void OnStep(Entity<MCXenoAdrenalinComponent> entity, ref MCMobStepEvent args)
    {
        entity.Comp.LastStep = _timing.CurTime;
        Dirty(entity);
    }
}
