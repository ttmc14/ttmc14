using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._MC;

[CVarDefs]
public sealed class MCConfigVars : CVars
{
    /**
     * Respawn
     */

    public static readonly CVarDef<float> MCRespawnMarinesActionCooldownMinutes =
        CVarDef.Create("mc.respawn_marines_action_delay_minutes", 10f, CVar.SERVER | CVar.REPLICATED);

    /**
     * Fire
     */

    public static readonly CVarDef<bool> MCFireResistOnDeath =
        CVarDef.Create("mc.fire_resist_on_death", true, CVar.SERVER | CVar.REPLICATED);

    /**
     * Stamina
     */

    public static readonly CVarDef<bool> MCStaminaDamageOnRun =
        CVarDef.Create("mc.stamina_damage_on_run", false, CVar.SERVER | CVar.REPLICATED);

    /**
     * Round schedule
     */

    public static readonly CVarDef<bool> MCRoundSchedule =
        CVarDef.Create("mc.round_schedule.enabled", false, CVar.SERVERONLY);

    public static readonly CVarDef<string> MCRoundScheduleTimezone =
        CVarDef.Create("mc.round_schedule.timezone", "Russian Standard Time", CVar.SERVERONLY);

    public static readonly CVarDef<float> MCRoundScheduleUpdateFrequency =
        CVarDef.Create("mc.round_schedule.update_frequency", 60f, CVar.SERVERONLY);

    public static readonly CVarDef<string> MCRoundScheduleDays =
        CVarDef.Create("mc.round_schedule.days", "Saturday,Sunday", CVar.SERVERONLY);

    public static readonly CVarDef<string> MCRoundScheduleStart =
        CVarDef.Create("mc.round_schedule.start", "17:00", CVar.SERVERONLY);

    public static readonly CVarDef<string> MCRoundScheduleEnd =
        CVarDef.Create("mc.round_schedule.end", "24:00", CVar.SERVERONLY);
}
