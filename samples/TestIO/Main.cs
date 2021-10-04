// Inspired by https://gist.github.com/louthy/524fbe8965d3a2aae1b576cdd8e971e4

using FreeAwait;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

await Test("rhyme.txt");

async Task Test(string path)
{
    await MonadicIOComputation(path, $"live-{path}").Use(new LiveInterpreter());
    await MonadicIOComputation(path, $"async-{path}").Use(new LiveInterpreterAsync());
    await MonadicIOComputation(path, $"mock-{path}").Use(new MockInterpreter());
}

async IStep<Void> MonadicIOComputation(string inPath, string outPath)
{
    var lines = (await new ReadAllLines(inPath)).ToList();
    await new Log($"There are {lines.Count} lines in {inPath}");
    await new Log("Prepending line numbers");
    var newLines = Enumerable.Range(1, int.MaxValue).Zip(lines).Select(item => $"{item.First} {item.Second}");
    await new WriteAllLines(outPath, newLines);
    await new Log($"Lines prepended and {outPath} saved successfully");
    return default;
}

struct Void { }

record ReadAllLines(string Path): IStep<ReadAllLines, IEnumerable<string>>;
record WriteAllLines(string Path, IEnumerable<string> Output): IStep<WriteAllLines, Void>;
record Log(string Output): IStep<Log, Void>;

interface IInterpreter:
    IRun<ReadAllLines, IEnumerable<string>>,
    IRun<WriteAllLines, Void>,
    IRun<Log, Void>
{ 
}

interface IInterpreterAsync: 
    IRunAsync<ReadAllLines, IEnumerable<string>>,
    IRunAsync<WriteAllLines, Void>,
    IRunAsync<Log, Void>
{ }

class LiveInterpreter : IInterpreter
{
    public IEnumerable<string> Run(ReadAllLines command) =>
        File.ReadAllLines(command.Path);

    public Void Run(WriteAllLines command)
    {
        File.WriteAllLines(command.Path, command.Output);
        return default;
    }

    public Void Run(Log command)
    {
        Console.WriteLine(command.Output);
        return default;
    }
}


class LiveInterpreterAsync : IInterpreterAsync
{
    public async Task<IEnumerable<string>> RunAsync(ReadAllLines command) =>
        await File.ReadAllLinesAsync(command.Path);

    public async Task<Void> RunAsync(WriteAllLines command)
    {
        await File.WriteAllLinesAsync(command.Path, command.Output);
        return default;
    }

    public Task<Void> RunAsync(Log command)
    {
        Console.WriteLine(command.Output);
        return Task.FromResult<Void>(default);
    }
}

class MockInterpreter : IInterpreter
{
    public IEnumerable<string> Run(ReadAllLines command) =>
        new[] { "Hello", "World" }.AsEnumerable();

    public Void Run(WriteAllLines _) => default;

    public Void Run(Log command)
    {
        Console.WriteLine(command.Output);
        return default;
    }
}