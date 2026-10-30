using Robust.Shared.Configuration;

namespace Content.Shared._MC._Config;

public sealed partial class MCConfigVars
{
    public static readonly CVarDef<int> XenoQueueJoinOfferTimeoutSeconds =
        CVarDef.Create("mc.xeno.queue_join.offer_timeout_seconds", 30, CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<int> XenoQueueJoinWaitSeconds =
        CVarDef.Create("mc.xeno.queue_join.wait_seconds", 300, CVar.REPLICATED | CVar.SERVER);
}
