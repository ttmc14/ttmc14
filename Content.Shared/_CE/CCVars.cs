using Robust.Shared.Configuration;

// ReSharper disable once CheckNamespace
namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<float>
        CEBaseFallingDamage = CVarDef.Create("zlevels.ce_base_falling_damage", 1.5f, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float>
        CEBaseFallingOtherDamage = CVarDef.Create("zlevels.ce_base_falling_other_damage", 0.1f, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float>
        CEBaseFallingStunTime = CVarDef.Create("zlevels.ce_base_falling_stun_time", 0f, CVar.SERVER | CVar.REPLICATED);

    public static readonly CVarDef<float>
        CEBaseFallingOtherStunTime = CVarDef.Create("zlevels.ce_base_falling_other_stun_time", 0f, CVar.SERVER | CVar.REPLICATED);
}
