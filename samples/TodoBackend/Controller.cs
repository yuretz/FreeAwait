﻿using FreeAwait;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoBackend
{
    [Route("api/todos/")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        [HttpGet()]
        public async IStep<IActionResult> Get() => Ok(await new ReadAll());

    }
}
