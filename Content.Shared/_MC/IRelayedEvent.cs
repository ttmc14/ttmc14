namespace Content.Shared._MC;

public interface IRelayedEvent<T>
{
    T Args { get; set; }
}

