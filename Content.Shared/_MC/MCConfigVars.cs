using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._MC;

[CVarDefs]
public sealed class MCConfigVars : CVars
{
    /**
     * Z-Levels
     */

    public static readonly CVarDef<bool> ZLevelsPhysicsClientSimulation =
        CVarDef.Create("mc.z_levels.physics.client_simulation", true, CVar.ARCHIVE | CVar.CLIENT);

    /**
     * Round
     */

    public static readonly CVarDef<int> RoundForceEndHijackTimeMinutes =
        CVarDef.Create("mc.round.hijack_end_time_minutes", 25, CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<bool> RoundCanEnd =
        CVarDef.Create("mc.round.can_end", true, CVar.REPLICATED | CVar.SERVER);

    /**
     * ASRS
     */

    public static readonly CVarDef<int> AsrsStartingBalance =
        CVarDef.Create("mc.asrs_starting_balance", 500, CVar.REPLICATED | CVar.SERVER);

    /**
     * Respawn
     */

    public static readonly CVarDef<float> RespawnMarinesActionCooldownMinutes =
        CVarDef.Create("mc.respawn_marines_action_delay_minutes", 10f, CVar.SERVER | CVar.REPLICATED);

    /**
     * Fire
     */

    public static readonly CVarDef<bool> FireResistOnDeath =
        CVarDef.Create("mc.fire_resist_on_death", true, CVar.SERVER | CVar.REPLICATED);

    /**
     * Stamina
     */

    public static readonly CVarDef<bool> StaminaDamageOnRun =
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
