// Inspired by https://gist.github.com/louthy/524fbe8965d3a2aae1b576cdd8e971e4

using FreeAwait;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

await Test(Path.Combine(Directory.GetCurrentDirectory(), "rhyme.txt"));

async Task Test(string path)
{
    await MonadicIOComputation(path).Use(new LiveIO());
    
    await MonadicIOComputation(path).Use(new MockIO());
}

async IStep<Void> MonadicIOComputation(string path)
{
    var lines = (await new ReadAllLines(path).Run()).ToList();
    await new Log($"There are {lines.Count} lines").Run();
    await new Log("Prepending line numbers").Run();
    var newLines = Enumerable.Range(1, int.MaxValue).Zip(lines).Select(item => $"{item.First} {item.Second}");
    await new WriteAllLines(path, newLines).Run();
    await new Log("Lines prepended and file saved successfully").Run();

    return default;
}

struct Void { }

record ReadAllLines(string Path): IStep<ReadAllLines, IEnumerable<string>>;
record WriteAllLines(string Path, IEnumerable<string> Output): IStep<WriteAllLines, Void>;
record Log(string Output): IStep<Log, Void>;

interface IIOInterpreter: 
    IRun<ReadAllLines, IEnumerable<string>>,
    IRun<WriteAllLines, Void>,
    IRun<Log, Void>
{ }

class LiveIO : IIOInterpreter
{
    public async Task<IEnumerable<string>> Run(ReadAllLines command) =>
        await File.ReadAllLinesAsync(command.Path);

    public async Task<Void> Run(WriteAllLines command)
    {
        await File.WriteAllLinesAsync(command.Path, command.Output);
        return default;
    }

    public Task<Void> Run(Log command)
    {
        Console.WriteLine(command.Output);
        return Task.FromResult<Void>(default);
    }

}

class MockIO : IIOInterpreter
{
    public Task<IEnumerable<string>> Run(ReadAllLines command) =>
        Task.FromResult(new[] { "Hello", "World" }.AsEnumerable());

    public Task<Void> Run(WriteAllLines command) => Task.FromResult<Void>(default);

    public Task<Void> Run(Log command)
    {
        Console.WriteLine(command.Output);
        return Task.FromResult<Void>(default);
    }
}