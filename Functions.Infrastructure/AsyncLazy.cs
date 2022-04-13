namespace Functions.Infrastructure;

public class AsyncLazy<T>
{
    private readonly Func<Task<T>> _factory;
    private          Task<T>?      _value;
    private object _lock = new object();

    public AsyncLazy(Func<Task<T>> factory)
    {
        _factory = factory;
    }

    public Task<T> Value
    {
        get
        {
            lock (_lock)
            {
                
                return _value ??= _factory();
            }
        }
    }
}