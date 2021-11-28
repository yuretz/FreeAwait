using FreeAwait;
using Microsoft.AspNetCore.Http;
using Void = FreeAwait.Void;
using System.Collections.Generic;

namespace MinimalWebApi.Tests;

public abstract class MockRunner :
    IRun<Read, Todo?>,
    IRun<ReadAll, IEnumerable<Todo>>,
    IRun<Create, Todo>,
    IRun<Locate, Todo>,
    IRun<Delete, Todo?>,
    IRun<DeleteAll, Void>,
    IRun<Update, Todo?>,
    IRun<HttpSteps.Ok, IResult>,
    IRun<HttpSteps.NotFound, IResult>,
    IRun<HttpSteps.NoContent, IResult>
{
    public abstract Todo? Run(Read step);
    public abstract IEnumerable<Todo> Run(ReadAll step);
    public abstract Todo Run(Create step);
    public abstract Todo? Run(Delete step);
    public abstract Void Run(DeleteAll step);
    public abstract Todo Run(Locate step);
    public abstract Todo? Run(Update step);
    public abstract IResult Run(HttpSteps.Ok step);
    public abstract IResult Run(HttpSteps.NotFound step);
    public abstract IResult Run(HttpSteps.NoContent step);
    
}

