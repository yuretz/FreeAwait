using Microsoft.AspNetCore.Http.Json;
using MinimalWebApi;
using System.Text.Json.Serialization;
using static Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddFreeAwait();
builder.Services.AddCors(options => 
    options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddSingleton<MinimalWebApi.Store>();

builder.Services.Configure<JsonOptions>(
    options => options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseCors();

// Map API endpoints
app.MapGet("/api/todos", () => Extensions.Run(Api.Get()));
app.MapGet("/api/todos/{id}", (int id) => Extensions.Run(Api.Get(id)));
app.MapPost("/api/todos", (Create create) => Extensions.Run(Api.Post(create)));
app.MapDelete("/api/todos", () => Extensions.Run(Api.Delete()));
app.MapDelete("/api/todos/{id}", (int id) => Extensions.Run(Api.Delete(id)));
app.MapMethods("/api/todos/{id}", new[] { "PATCH" }, (int id, Patch patch) => Extensions.Run(Api.Patch(id, patch)));


app.Run();

