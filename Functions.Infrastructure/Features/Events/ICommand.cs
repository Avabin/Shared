namespace Functions.Infrastructure.Features.Events;

public interface ICommand : IEvent
{
    
}

public abstract class Command : EventBase, ICommand
{
}