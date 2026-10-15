using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._MC._Config;

[CVarDefs]
public sealed partial class MCConfigVars : CVars
{
    public static readonly CVarDef<bool> QoLRunAlert =
        CVarDef.Create("mc.qol.run_alert", true, CVar.ARCHIVE | CVar.CLIENT);

    public static readonly CVarDef<bool> ChatEmoji =
        CVarDef.Create("mc.chat.emoji", true, CVar.ARCHIVE | CVar.CLIENT);

    /**
     * UI
     */

    public static readonly CVarDef<bool> UISlotControlClipping =
        CVarDef.Create("mc.ui.slot_control_clipping", true, CVar.ARCHIVE | CVar.CLIENT);

    public static readonly CVarDef<bool> UIRadialSpawnAtMouse =
        CVarDef.Create("mc.ui.radial_mouse_coords", true, CVar.ARCHIVE | CVar.CLIENT);

    /**
     * Z-Levels
     */

    public static readonly CVarDef<float> ZLevelsOffset =
        CVarDef.Create("mc.z_levels.offset", 0f, CVar.ARCHIVE | CVar.CLIENT);

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
     * Fire
     */

    public static readonly CVarDef<bool> FireResistOnDeath =
        CVarDef.Create("mc.fire_resist_on_death", true, CVar.SERVER | CVar.REPLICATED);
}
