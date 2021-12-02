# FreeAwait
[![.NET](https://github.com/yuretz/FreeAwait/actions/workflows/build.yml/badge.svg)](https://github.com/yuretz/FreeAwait/actions/workflows/build.yml)

Await anything for free!

## Purpose
FreeAwait is a tiny .NET library implementing a free monad-like pattern with C# async/await. It can be used as a more [functional alternative to dependency injection](https://blog.ploeh.dk/2017/01/27/from-dependency-injection-to-dependency-rejection/), that comes without the need to give up on the good old idiomatic C# code style.

## Quick example
Lets start with adding the FreeAwait library to our using directives:
```csharp
using FreeAwait;
using Void = FreeAwait.Void;
```
The second line gives us an actually useful `Void` type, instead of [the fake one](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Void.cs), but feel free to substitute it with [any](https://github.com/louthy/language-ext/blob/main/LanguageExt.Core/DataTypes/Unit/Unit.cs) [other](https://github.com/jbogard/MediatR/blob/master/src/MediatR/Unit.cs) [alternative](https://github.com/dotnet/reactive/blob/main/Rx.NET/Source/src/System.Reactive/Unit.cs), or just roll your own, and contribute to the eventual heat death of the universe like everybody else does.

Now we can declare the following types:
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
* Some static and extension methods that might come in handy. When needed, you can utilize them
  * to turn any value into an `IStep`
    ```csharp
    IStep<TResult> Step.Result<TResult>(TResult value)
    ```
  * to pass step result into a function:
    ```csharp
    IStep<TNext> IStep<TResult>.PassTo<TResult, TNext>( 
            Func<TResult, IStep<TNext>> next)
    ```
  * to turn an `IEnumerable<IStep<T>>` into an `IStep<IAsyncEnumerable<T>>`:
    ```csharp
    IStep<IAsyncEnumerable<T>> IEnumerable<IStep<T>>.Sequence<T>()
    ```

## ASP.NET extensions
To be documented

## Reasons to use
- control over side effects, loose coupling and better testability;
- code that looks more idiomatic, than something you could achieve by composing LINQ expressions, as suggested by other good libraries;
- it's free, freedom is worth a wait (terrible pun, sorry).
