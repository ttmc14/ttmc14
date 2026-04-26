using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Constructions.Silo;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoSiloComponent : Component
{
    private const double SiloOutputPonderation = 2.0;
    private const double SiloBaseOutputPerMarine = 1.5;
    private const double HijackMultiplier = 3.0;
    private const double RationMultiplierMin = 0.5;
    private const double RationMultiplierMax = 2.0;
    private const double OptimalRatio = 1;

    public static double CalculateLarvaSpawnRate(
        int siloCount,
        int activeHumans,
        int activeXenos,
        bool hijacked,
        double siloScaling)
    {
        // Base output based on the number of silos
        var spawnRate = siloCount > 0
            ? SiloOutputPonderation + siloCount
            : 0;

        // Scale to people
        spawnRate *= SiloBaseOutputPerMarine * activeHumans;

        // Normalization for a single silo
        spawnRate /= 1.0 + SiloOutputPonderation;

        // Hijack bonus
        spawnRate *= hijacked ? HijackMultiplier : 1.0;

        // Mode factor
        spawnRate *= siloScaling;

        // Balance based on the humans-to-xenos ratio
        var currentRatio = (double)activeHumans / activeXenos;

        var ratioMultiplier = double.Round(currentRatio / OptimalRatio, 2, MidpointRounding.AwayFromZero);
        ratioMultiplier = double.Clamp(ratioMultiplier, RationMultiplierMin, RationMultiplierMax);

        spawnRate *= ratioMultiplier;

        return spawnRate;
    }
}
