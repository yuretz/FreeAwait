using static FreeAwait.HttpSteps;
namespace MinimalWebApi;

public static class Api
{
    public static async IStep<IAsyncEnumerable<Todo>> Get() =>
        await (await new ReadAll())
                .Select(item => new Locate(item))
                .Sequence();

    public static async IStep<Todo> Post(Create create) => await new Locate(await create);

    public static async IStep<IResult> Get(int id) =>
            await new Read(id) is { } item
                ? await new Ok(await new Locate(item))
                : await new NotFound();

    public static async IStep<IResult> Delete(int id) =>
            await new Delete(id) is { } item
                ? await new Ok(await new Locate(item))
                : await new NotFound();

    public static IStep<IResult> Delete() => new DeleteAll().PassTo(_ => new NoContent());

    public static async IStep<IResult> Patch(int id, Patch patch) =>
            await new Update(id, patch) is { } item
                ? await new Ok(await new Locate(item))
                : await new NotFound();
}   