using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoBackend
{
    public record Todo(int Id, string Title, bool Completed, int? Order = default, string? Url = default);
}
