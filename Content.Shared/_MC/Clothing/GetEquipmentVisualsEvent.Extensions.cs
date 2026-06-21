using Robust.Shared.Serialization;

// ReSharper disable once CheckNamespace
namespace Content.Shared.Clothing;

public sealed partial class GetEquipmentVisualsEvent
{
    public Dictionary<string, string?> MCDepth = new();
}
