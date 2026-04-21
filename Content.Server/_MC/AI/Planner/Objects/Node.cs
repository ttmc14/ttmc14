namespace Content.Server._MC.AI.Planner.Objects;

public struct Node
{
    public int State;
    public int Parent;
    public int ActionIndex;

    public float G;
    public float H;
}
