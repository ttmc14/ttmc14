using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Armor.Core.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCArmorComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool ShowExamine = true;

    [DataField, AutoNetworkedField]
    public MCArmorDefinition Soft;

    [DataField, AutoNetworkedField]
    public MCArmorDefinition Hard;
}

[DataDefinition, Serializable, NetSerializable]
public partial struct MCArmorDefinition : IEquatable<MCArmorDefinition>
{
    [DataField] public int Melee;
    [DataField] public int Bullet;
    [DataField] public int Laser;
    [DataField] public int Energy;
    [DataField] public int Bomb;
    [DataField] public int Bio;
    [DataField] public int Fire;
    [DataField] public int Acid;
    [DataField] public int Fall;

    public bool Equals(MCArmorDefinition other)
    {
        return Melee == other.Melee
            && Bullet == other.Bullet
            && Laser == other.Laser
            && Energy == other.Energy
            && Bomb == other.Bomb
            && Bio == other.Bio
            && Fire == other.Fire
            && Acid == other.Acid
            && Fall == other.Fall;
    }

    public override bool Equals(object? obj)
    {
        return obj is MCArmorDefinition other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Melee);
        hash.Add(Bullet);
        hash.Add(Laser);
        hash.Add(Energy);
        hash.Add(Bomb);
        hash.Add(Bio);
        hash.Add(Fire);
        hash.Add(Acid);
        hash.Add(Fall);

        return hash.ToHashCode();
    }

    public static bool operator ==(MCArmorDefinition left, MCArmorDefinition right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MCArmorDefinition left, MCArmorDefinition right)
    {
        return !left.Equals(right);
    }

    public static MCArmorDefinition operator +(MCArmorDefinition left, int right)
    {
        return new MCArmorDefinition
        {
            Melee = left.Melee + right,
            Bullet = left.Bullet + right,
            Laser = left.Laser + right,
            Energy = left.Energy + right,
            Bomb = left.Bomb + right,
            Bio = left.Bio + right,
            Fire = left.Fire + right,
            Acid = left.Acid + right,
            Fall = left.Fall + right,
        };
    }

    public static MCArmorDefinition operator +(MCArmorDefinition left, MCArmorDefinition right)
    {
        return new MCArmorDefinition
        {
            Melee = left.Melee + right.Melee,
            Bullet = left.Bullet + right.Bullet,
            Laser = left.Laser + right.Laser,
            Energy = left.Energy + right.Energy,
            Bomb = left.Bomb + right.Bomb,
            Bio = left.Bio + right.Bio,
            Fire = left.Fire + right.Fire,
            Acid = left.Acid + right.Acid,
            Fall = left.Fall + right.Fall,
        };
    }

    public static MCArmorDefinition operator -(MCArmorDefinition left, int right)
    {
        return new MCArmorDefinition
        {
            Melee = left.Melee - right,
            Bullet = left.Bullet - right,
            Laser = left.Laser - right,
            Energy = left.Energy - right,
            Bomb = left.Bomb - right,
            Bio = left.Bio - right,
            Fire = left.Fire - right,
            Acid = left.Acid - right,
            Fall = left.Fall - right,
        };
    }

    public static MCArmorDefinition operator -(MCArmorDefinition left, MCArmorDefinition right)
    {
        return new MCArmorDefinition
        {
            Melee = left.Melee - right.Melee,
            Bullet = left.Bullet - right.Bullet,
            Laser = left.Laser - right.Laser,
            Energy = left.Energy - right.Energy,
            Bomb = left.Bomb - right.Bomb,
            Bio = left.Bio - right.Bio,
            Fire = left.Fire - right.Fire,
            Acid = left.Acid - right.Acid,
            Fall = left.Fall - right.Fall,
        };
    }

    public static MCArmorDefinition operator *(MCArmorDefinition left, float right)
    {
        return new MCArmorDefinition
        {
            Melee = (int) (left.Melee * right),
            Bullet = (int) (left.Bullet * right),
            Laser = (int) (left.Laser * right),
            Energy = (int) (left.Energy * right),
            Bomb = (int) (left.Bomb * right),
            Bio = (int) (left.Bio * right),
            Fire = (int) (left.Fire * right),
            Acid = (int) (left.Acid * right),
            Fall = (int) (left.Fall * right),
        };
    }
}
