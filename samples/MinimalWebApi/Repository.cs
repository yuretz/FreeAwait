
namespace MinimalWebApi;

public class Repository :
    IRun<Create, Todo>,
    IRun<Read, Todo?>,
    IRun<ReadAll, IEnumerable<Todo>>,
    IRun<Delete, Todo?>,
    IRun<DeleteAll, Void>,
    IRun<Update, Todo?>

{
    public Repository(Store store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public IEnumerable<Todo> Run(ReadAll step) => _store.Todos.Select(pair => pair.Value).OrderBy(item => item.Order);

    public Todo Run(Create step)
    {
        var item = new Todo(_store.NextId(), step.Title, false, step.Order);
        _store.Todos[item.Id] = item;

        return item;
    }

    public Todo? Run(Read step) => _store.Todos.TryGetValue(step.Id, out var item) ? item : default;

    public Todo? Run(Delete step) => _store.Todos.Remove(step.Id, out var item) ? item : default;

    public Void Run(DeleteAll step)
    {
        _store.Todos.Clear();
        return default;
    }

    public Todo? Run(Update step)
    {
        if (!_store.Todos.TryGetValue(step.Id, out var item))
        {
            return default;
        }

        item = item with
        {
            Completed = step.Patch.Completed ?? item.Completed,
            Order = step.Patch.Order ?? item.Order,
            Title = step.Patch.Title ?? item.Title
        };

        _store.Todos[step.Id] = item;

        return item;
    }

    private readonly Store _store;
}
