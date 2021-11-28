using System.Collections.Concurrent;
namespace MinimalWebApi;

public class Store
{
    public readonly ConcurrentDictionary<int, Todo> Todos = new();

    public int NextId() => Interlocked.Increment(ref _id);

    private int _id; 
}

