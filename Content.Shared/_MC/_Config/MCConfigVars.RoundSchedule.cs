using Robust.Shared.Configuration;

namespace Content.Shared._MC._Config;

public sealed partial class MCConfigVars
{
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
