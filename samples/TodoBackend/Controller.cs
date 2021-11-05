using FreeAwait;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoBackend
{
    [Route("api/todos")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        [HttpPost]
        public async IStep<IActionResult> Post([FromBody] Create create) => Ok(await Locate(create));

        [HttpGet("{id}")]
        public async IStep<IActionResult> Get([FromRoute] int id) => 
            await new Read(id) is { } item 
                ? Ok(await Locate(item)) 
                : NotFound();

        [HttpGet]
        public async IStep<IActionResult> Get() => Ok(
            await (await new ReadAll())
                .Select(Locate)
                .Sequence());

        [HttpDelete("{id}")]
        public async IStep<IActionResult> Delete([FromRoute] int id) =>
            await new Delete(id) is { } item
                ? Ok(await Locate(item))
                : NotFound();

        [HttpDelete]
        public async IStep<IActionResult> Delete()
        {
            await new DeleteAll();
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async IStep<IActionResult> Patch([FromRoute] int id, [FromBody] Patch patch) => 
            await new Update(id, patch) is { } item
                ? Ok(await Locate(item))
                : NotFound();

        private static IStep<Todo> Locate(Todo item) => new Locate(item);

        private static async IStep<Todo> Locate(IStep<Todo> step) => await new Locate(await step);
    }
}
