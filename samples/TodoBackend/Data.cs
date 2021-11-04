using FreeAwait;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoBackend
{
    public record Todo(int Id, string Title, bool Completed, int? Order = default, string? Url = default);
    
    public record Read(int Id): IStep<Read, Todo?>;

    public record Create(string Title, int? Order) : IStep<Create, Todo>;

    public record ReadAll() : IStep<ReadAll, IEnumerable<Todo>>;

    public record Locate(Todo Item) : IStep<Locate, Todo>;
}
