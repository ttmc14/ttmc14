using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._MC;

[CVarDefs]
public sealed class MCConfigVars : CVars
{
    public static readonly CVarDef<bool> ChatEmoji =
        CVarDef.Create("mc.chat.emoji", true, CVar.ARCHIVE | CVar.CLIENT);


    /**
     * Z-Levels
     */

    public static readonly CVarDef<int> ZLevelsPhysicsTickRate =
        CVarDef.Create("mc.z_levels.physics.tick_rate", 60, CVar.ARCHIVE);

    public static readonly CVarDef<bool> ZLevelsPhysicsClientSimulation =
        CVarDef.Create("mc.z_levels.physics.client_simulation", true, CVar.ARCHIVE | CVar.CLIENT);

    /**
     * Vote
     */

    public static readonly CVarDef<bool> VoteEnabled =
        CVarDef.Create("mc.vote.enabled", true, CVar.SERVER | CVar.SERVERONLY);

    public static readonly CVarDef<int> VoteExcludeLast =
        CVarDef.Create("mc.vote.exclude_last", 2, CVar.SERVER | CVar.SERVERONLY);

    public static readonly CVarDef<bool> VoteCarryover =
        CVarDef.Create("mc.vote.carryover", true, CVar.SERVER | CVar.SERVERONLY);

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
