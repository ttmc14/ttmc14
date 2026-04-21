using Robust.Shared.Serialization;

namespace Content.Shared._MC.AI;

[Serializable, NetSerializable]
public sealed partial class MCAIMemory
{
    public void Clear()
    {
        StateClear();
        ContainerClear();
    }
}
