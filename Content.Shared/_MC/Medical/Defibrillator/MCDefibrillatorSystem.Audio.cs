using Content.Shared._MC.Medical.Defibrillator.Components;

namespace Content.Shared._MC.Medical.Defibrillator;

public sealed partial class MCDefibrillatorSystem
{
    public void StartChargingAudio(Entity<MCDefibrillatorComponent> entity)
    {
        StopChargingAudio(entity);
        entity.Comp.EffectSoundChargeEntity = _audio.PlayPvs(entity.Comp.EffectSoundCharge, entity)?.Entity;
    }

    public void StopChargingAudio(Entity<MCDefibrillatorComponent> entity)
    {
        _audio.Stop(entity.Comp.EffectSoundChargeEntity);
        PredictedQueueDel(entity.Comp.EffectSoundChargeEntity);

        entity.Comp.EffectSoundChargeEntity = null;
    }
}
