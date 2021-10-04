# FreeAwait
Await anything for free!

## What
FreeAwait is a tiny .NET library implementing a free monad-like pattern with C# async/await. It can be used as a more [functional alternative to dependency injection](https://blog.ploeh.dk/2017/01/27/from-dependency-injection-to-dependency-rejection/), that comes without the need to give up on the good old idiomatic C# code style.

## How
Lets start with adding the FreeAwait library to our using directives:
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

## Why
- control over side effects, loose coupling and better testability;
- code that looks more idiomatic, than something you could achieve by composing LINQ expressions, as suggested by other good libraries;
- it's free, freedom is worth a wait (terrible pun, sorry).
