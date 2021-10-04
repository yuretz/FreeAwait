using System;
using FreeAwait;

await new ConsoleIO().Run(Talk());

async IStep<Void> Talk()
{
    await new WriteLine("Let's talk...");
    var name = await Greet();
    await new WriteLine($"Just talked to {name}.");

    return default;
}

async IStep<string?> Greet()
{
    await new WriteLine("What's your name, stranger?");
    var name = await new ReadLine();
    await new WriteLine($"Greetings, {name}!");
    return name;
}

struct Void { }
record ReadLine: IStep<ReadLine, string?>;
record WriteLine(string? Text): IStep<WriteLine, Void>;

class ConsoleIO :
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


