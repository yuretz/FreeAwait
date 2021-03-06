namespace MinimalWebApi;

public class Locator : IRun<Locate, Todo>
{
    public Locator(IHttpContextAccessor context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private readonly IHttpContextAccessor _context;

    public Todo Run(Locate step) => _context.HttpContext?.Request is { } request
       ? step.Item with { Url = $"{request.Scheme}://{request.Host}/api/todos/{step.Item.Id}" }
       : step.Item;
}
