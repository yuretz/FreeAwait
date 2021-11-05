using FreeAwait;
using System.Collections.Generic;

namespace TodoBackend
{
    public record Todo(int Id, string Title, bool Completed, int? Order = default, string? Url = default);

    public record Create(string Title, int? Order): IStep<Create, Todo>;

    public record Read(int Id): IStep<Read, Todo?>;

    public record ReadAll(): IStep<ReadAll, IEnumerable<Todo>>;

    public record Delete(int Id): IStep<Delete, Todo?>;

    public record DeleteAll(): IStep<DeleteAll, Void>;

    public record Locate(Todo Item): IStep<Locate, Todo>;

    public record Patch(string? Title, bool? Completed, int? Order);
    public record Update(int Id, Patch patch): IStep<Update, Todo?>;
}
