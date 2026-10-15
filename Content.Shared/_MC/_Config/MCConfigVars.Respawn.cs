using Robust.Shared.Configuration;

namespace Content.Shared._MC._Config;

public sealed partial class MCConfigVars
{
    public static readonly CVarDef<float> RespawnMarinesActionCooldownMinutes =
        CVarDef.Create("mc.respawn_marines_action_delay_minutes", 10f, CVar.SERVER | CVar.REPLICATED);
}
