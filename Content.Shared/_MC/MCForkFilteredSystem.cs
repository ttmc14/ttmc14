using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._MC;

public sealed class MCForkFilteredSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = null!;

    public override void Initialize()
    {
        _configuration.SetCVar(CVars.EntitiesCategoryFilter, "MCContent");
    }
}
