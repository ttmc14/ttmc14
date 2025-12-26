using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Abilities.Puppeteer;

[Serializable, NetSerializable]
public enum MCXenoPuppetSelectorUi : byte
{
    Key
}

[Serializable, NetSerializable]
public enum MCXenoPuppetBlessingSelectorUi : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class MCXenoPuppetBlessingChosenBuiMsg : BoundUserInterfaceMessage
{
    public MCXenoPuppetBlessing Blessing;

    public MCXenoPuppetBlessingChosenBuiMsg(MCXenoPuppetBlessing blessing)
    {
        Blessing = blessing;
    }
}

[Serializable, NetSerializable]
public sealed class MCXenoPuppetChosenBuiMsg : BoundUserInterfaceMessage
{
    public NetEntity Puppet;

    public MCXenoPuppetChosenBuiMsg(NetEntity puppet)
    {
        Puppet = puppet;
    }
}
