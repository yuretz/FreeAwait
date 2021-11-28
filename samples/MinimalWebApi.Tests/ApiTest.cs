using Xunit;
using NSubstitute;
using FreeAwait;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Void = FreeAwait.Void;
using System.Linq;

namespace MinimalWebApi.Tests;

public class ApiTest
{
    public ApiTest()
    {
        _runner = Substitute.For<MockRunner>();
        _runner.Run(Arg.Any<Locate>()).Returns(args => Located(((Locate)args[0]).Item));
    }

    [Fact]
    public async Task GetExisting()
    {
        _runner.Run(new Read(_specimen[0].Id)).Returns(_specimen[0]);
        _runner.Run(new HttpSteps.Ok(Located(_specimen[0]))).Returns(_result);

        Assert.Equal(_result, await Api.Get(_specimen[0].Id).Use(_runner));
    }

    [Fact]
    public async Task GetAll()
    {
        _runner.Run(new ReadAll()).Returns(_specimen);

        Assert.Equal(_specimen.Select(Located), (await Api.Get().Use(_runner)).ToEnumerable());
    }

    [Fact]
    public async Task GetNonExisting()
    {
        _runner.Run(new Read(_specimen[0].Id)).Returns((Todo?)null);
        _runner.Run(Arg.Any<HttpSteps.NotFound>()).Returns(_result);

        Assert.Equal(_result, await Api.Get(_specimen[0].Id).Use(_runner));
    }

    [Fact]
    public async Task PostNew()
    {
        _runner.Run(new Create(_specimen[0].Title, default)).Returns(_specimen[0]);

        Assert.Equal(Located(_specimen[0]), await Api.Post(new Create(_specimen[0].Title, default)).Use(_runner));
    }

    [Fact]
    public async Task DeleteOne()
    {
       _runner.Run(new Delete(_specimen[0].Id)).Returns(_specimen[0]);
       _runner.Run(new HttpSteps.Ok(Located(_specimen[0]))).Returns(_result);

        Assert.Equal(_result, await Api.Delete(_specimen[0].Id).Use(_runner));
    }

    [Fact]
    public async Task DeleteAll()
    {
        _runner.Run(new DeleteAll()).Returns(default(Void));
        _runner.Run(Arg.Any<HttpSteps.NoContent>()).Returns(_result);

        Assert.Equal(_result, await Api.Delete().Use(_runner));
    }

    [Fact]
    public async Task PatchExisting()
    {
        var patch = new Patch(_specimen[0].Title, _specimen[0].Completed, _specimen[0].Order);

        _runner.Run(new Update(_specimen[0].Id, patch)).Returns(_specimen[0]);
        _runner.Run(new HttpSteps.Ok(Located(_specimen[0]))).Returns(_result);

        Assert.Equal(_result, await Api.Patch(_specimen[0].Id, patch).Use(_runner));
    }

    [Fact]
    public async Task PatchNonExisting()
    {
        var patch = new Patch(_specimen[0].Title, _specimen[0].Completed, _specimen[0].Order);

        _runner.Run(new Update(_specimen[0].Id, patch)).Returns((Todo?)null);
        _runner.Run(Arg.Any<HttpSteps.NotFound>()).Returns(_result);

        Assert.Equal(_result, await Api.Patch(_specimen[0].Id, patch).Use(_runner));
    }

    private readonly Todo[] _specimen =
    {
        new(42, "Foo", false),
        new(123, "Bar", true),
        new(321, "Baz", false, 5)
    };

    private Todo Located(Todo item) => item with { Url = item.Id.ToString() };

    private readonly MockRunner _runner = Substitute.For<MockRunner>();
    private readonly IResult _result = Substitute.For<IResult>();
}
