using Robust.Shared.Utility;

namespace Content.Server._MC.GridLoader;

[RegisterComponent]
public sealed partial class MCGridLoaderComponent : Component
{
    [DataField]
    public ResPath Map;

    [DataField]
    public string? ForceFtlKey;
}
