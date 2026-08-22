namespace Content.Shared._MC.UserInterface.Ensuring;

public sealed class MCUserInterfaceEnsuringSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCUserInterfaceEnsuringComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<MCUserInterfaceEnsuringComponent> entity, ref ComponentStartup args)
    {
        foreach (var (type, value) in entity.Comp.Interfaces)
        {
            _userInterface.SetUi(entity.Owner, type, value);
        }
    }
}
