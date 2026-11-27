using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.RoundInfo;

[Serializable, NetSerializable]
public enum MCRoundInfoLayout : byte
{
    Standard,
    Warfare,
}

[Serializable, NetSerializable]
public sealed class MCRoundInfoReserves
{
    public int? Marines;
    public int? Xenos;
}

[Serializable, NetSerializable]
public sealed class MCRoundInfoState(MCRoundInfoLayout layout, LocId title, LocId description, List<string> briefing) : EuiStateBase
{
    public MCRoundInfoLayout Layout { get; } = layout;
    public LocId Title { get; } = title;
    public LocId Description { get; } = description;
    public List<string> Briefing { get; } = briefing;
    public MCRoundInfoReserves Reserves { get; } = new();
}
