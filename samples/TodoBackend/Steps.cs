using FreeAwait;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoBackend
{
    public record ReadAll(): IStep<ReadAll, IEnumerable<Todo>>;

    public record Read(int Id): IStep<Read, Todo>;

}
