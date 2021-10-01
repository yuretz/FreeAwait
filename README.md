# FreeAwait
Await anything for free!

## What
A tiny .NET library implementing a free monad-like pattern with C# async/await.

## How
We start with adding the FreeAwait library to our usings:
```csharp
using FreeAwait;
```

Now we can declare the following types:
```csharp
struct Void {}
record ReadLine: IStep<ReadLine, string?>;
record WriteLine(string? Text): IStep<WriteLine, Void>;
```
These are our program "steps" (instructions) that we want to be processed by some external "runner" (interpreter).

So, with all that in place, our program could look like this:
```csharp
async IStep<string?> Greet()
{
    await new WriteLine("What's your name, stranger?").Run();
    var name = await new ReadLine().Run();
    await new WriteLine($"Hello {name}!").Run();
    return name;
}
```

Remember, all these `ReadLine()` and `WriteLine()` are some inanimate data structures we just declared, so they won't do anything on their own, but they look like the real thing, right? To bring the entire construct to life, what we need now is a runner that knows how to handle our program steps. Well, let's implement one:
```csharp
class ConsoleIO :
    IRun<ReadLine, string?>,
    IRun<WriteLine, Void>
{
    public Task<string?> Run(ReadLine command) => 
        Task.FromResult(Console.ReadLine());
    
    public Task<Void> Run(WriteLine command)
    {
        Console.WriteLine(command.Text);
        return Task.FromResult(default(Void));
    }
}
```

With all that, we are now able to run your program:
```csharp
var name = await Greet().Use(new ConsoleIO());
```

You can find more demo code in [samples](./samples).

## Why
- purity, loose-coupling and testability;
- code that looks more idiomatic, than something you could achieve by composing LINQ expressions, as suggested by other good libraries;
- it's free, freedom is worth a wait (terrible pun, sorry).
