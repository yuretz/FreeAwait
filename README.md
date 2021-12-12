# FreeAwait
[![.NET](https://github.com/yuretz/FreeAwait/actions/workflows/build.yml/badge.svg)](https://github.com/yuretz/FreeAwait/actions/workflows/build.yml)
[![Nuget](https://img.shields.io/nuget/v/FreeAwait)](https://www.nuget.org/packages/FreeAwait/)
![GitHub](https://img.shields.io/github/license/yuretz/FreeAwait)

Await anything for free!

## Purpose
FreeAwait is a tiny .NET library implementing a free monad-like pattern with C# async/await. It can be used as a more [functional alternative to dependency injection](https://blog.ploeh.dk/2017/01/27/from-dependency-injection-to-dependency-rejection/), that comes without the need to give up on the good old idiomatic C# code style.

## Reasons to use
* :heavy_check_mark: control over side effects, loose coupling and better testability;
* :scroll: code looks more idiomatic than composed LINQ expressions suggested by other good libraries;
* :hourglass: it's free, freedom is worth a wait (terrible pun, sorry).

## Quick example
Lets start with installing the [`FreeAwait` package from NuGet](https://www.nuget.org/packages/FreeAwait/) and adding to our using directives:
```csharp
using FreeAwait;
using Void = FreeAwait.Void;
```
The second line gives us an actually useful `Void` type, instead of [the fake one](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Void.cs), but feel free to substitute it with [any](https://github.com/louthy/language-ext/blob/main/LanguageExt.Core/DataTypes/Unit/Unit.cs) [other](https://github.com/jbogard/MediatR/blob/master/src/MediatR/Unit.cs) [alternative](https://github.com/dotnet/reactive/blob/main/Rx.NET/Source/src/System.Reactive/Unit.cs), or just roll your own, and contribute to the eventual heat death of the universe like everybody else does.

Anyway, now we can declare the following types:
```csharp
record ReadLine: IStep<ReadLine, string?>;
record WriteLine(string? Text): IStep<WriteLine, Void>;
```
These are our program "steps" (instructions) that we want to be processed by some external "runner" (interpreter).

So, with all that in place, our program could look like this:
```csharp
async IStep<string?> Greet()
{
    await new WriteLine("What's your name, stranger?");
    var name = await new ReadLine();
    await new WriteLine($"Greetings, {name}!");
    return name;
}
```

Remember, all these `ReadLine()` and `WriteLine()` are some inanimate data structures we just declared, so they won't do anything on their own, but they look like the real thing, right? To bring the entire construct to life, what we need now is a runner that knows how to handle our program steps. Well, let's implement one:
```csharp
class ConsoleIO:
    IRun<ReadLine, string?>,
    IRun<WriteLine, Void>
{
    public string? Run(ReadLine command) => Console.ReadLine();
   
    public Void Run(WriteLine command)
    {
        Console.WriteLine(command.Text);
        return default;
    }
}
```

With all that, we are now able to run our program:
```csharp
var name = await new ConsoleIO().Run(Greet());
```

You can find more demo code in [samples](./samples).

## More Features
* Asyncronous step runners are implemented via `IRunAsync<TStep, TResult>` interface like this: 
    ```csharp
    record ReadTextFile(string FileName): IStep<ReadTextFile, string>;

    class AsyncIO: IRunAsync<ReadTextFile, string>
    {
        public Task<string> RunAsync(ReadTextFile step) => 
            File.ReadAllTextAsync(step.FileName);
    }
    ```
* Recursive step runners are supported via `IRunStep<TStep, TResult>`, for example here is a recursive factorial step:
    ```csharp
    record Factor(int N): IStep<Factor, int>;
    
    class FactorRunner: IRunStep<Factor, int>
    {
        public async IStep<int> RunStep(Factor step) => 
            step.N <= 1 ? 1 : await new Factor(step.N - 1) * step.N;
    }
    ```
* Tail recursion is automagically trampolined (i.e. translated into a loop), so the following recursive fibonacci computation will not blow up the stack, even if called with very large N:
    ```csharp

    record Fib(int N, long Current = 1, long Previous = 0) : IStep<Fib, long>;
    
    class RecursiveRunner: IRunStep<Fib, long>
    {
        public IStep<long> RunStep(Fib step) => step.N <= 1
            ? Step.Result(step.Current)
            : new Fib(step.N - 1, step.Current + step.Previous, step.Current);
    }
    ```
* Some static and extension methods that might come in handy. When needed, you can utilize them to
  * turn any value into an `IStep`
    ```csharp
    IStep<TResult> Step.Result<TResult>(TResult value)
    ```
  * pass step result into a function:
    ```csharp
    IStep<TNext> IStep<TResult>.PassTo<TResult, TNext>( 
            Func<TResult, IStep<TNext>> next)
    ```
  * turn an `IEnumerable<IStep<T>>` into an `IStep<IAsyncEnumerable<T>>`:
    ```csharp
    IStep<IAsyncEnumerable<T>> IEnumerable<IStep<T>>.Sequence<T>()
    ```

## ASP.NET extensions
After adding a reference to [`FreeAwait.Extensions.AspNetCore` from NuGet](https://www.nuget.org/packages/FreeAwait.Extensions.AspNetCore/) and a usual `using FreeAwait` directive, add the following line to your dependency registration code in `Program.cs`, like this:
```csharp
builder.Services.AddFreeAwait();
```
or if you are using traditional `Startup` class, add this to the `ConfigureServices()` method:
```csharp
services.AddFreeAwait();
```
This registers all exisiting runner classes and makes them available via a universal `IServiceRunner` interface which you can inject into your classes wherever you need it.

It also registers a global MVC action filter allowing you to return `IStep<IActionResult>` from your controller actions, pretty neat, huh? Take a look at a full [MVC Todo backend example](./samples/TodoBackend). 

If you are a fan of the new ASP.NET Core minimal web API approach, great news for you: it is [fully supported too](./samples/MinimalWebApi).

The only thing you need to do in order to use `IStep` returning methods as an endpoint handlers, is to pass it through `Results.Extensions.Run()` helper method. 
But why is it needed, you ask? Because [unit testing minimal web APIs in isolation is difficult](https://youtu.be/VuFQtyRmS0E)! Well, you can see yourself how FreeAwait makes it a piece of cake: take look at a [full unit test for the minimal Todo API](./samples/MinimalWebApi.Tests) and don't worry about `WebApplicationFactory`, because you might not need it.

## Futher info
If you have a question, or think you found a bug, or have a good idea for a feature and don't mind sharing it, please open an issue and I would be happy to discuss it.