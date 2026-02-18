namespace Content.Shared._MC.Mob.Movement;

[ByRefEvent]
public readonly record struct MCMobStepEvent(float FrameTime, bool Sprinting);
