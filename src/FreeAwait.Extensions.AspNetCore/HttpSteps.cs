#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FreeAwait
{
    public static class HttpSteps
    {
        public record Ok(object? Value = default) : IStep<Ok, IResult>;
        public record NotFound(object? Value = default) : IStep<NotFound, IResult>;
        public record NoContent(): IStep<NoContent, IResult>;
        public record Challenge(AuthenticationProperties? Properties = null, IList<string>? AuthenticationSchemes = null) : IStep<Challenge, IResult>;
        public record Forbid(AuthenticationProperties? Properties = null, IList<string>? AuthenticationSchemes = null) : IStep<Forbid, IResult>;
        public record SignIn(ClaimsPrincipal Principal, AuthenticationProperties? Properties = null, string? AuthenticationScheme = null) : IStep<SignIn, IResult>;
        public record SignOut(AuthenticationProperties? Properties = null, IList<string>? AuthenticationSchemes = null) : IStep<SignOut, IResult>;
        public record Text(string Content, string? ContentType = null, Encoding? ContentEncoding = null) : IStep<Text, IResult>;   
        public record Json(object? Data, JsonSerializerOptions? Options = null, string? ContentType = null, int? StatusCode = null) : IStep<Json, IResult>;
        public record Bytes(byte[] Contents, string? ContentType = null, string? FileDownloadName = null, bool EnableRangeProcessing = false, DateTimeOffset? LastModified = null, EntityTagHeaderValue? EntityTag = null) : IStep<Bytes, IResult>;
        public record Stream(System.IO.Stream Contents, string? ContentType = null, string? FileDownloadName = null, DateTimeOffset? LastModified = null, EntityTagHeaderValue? EntityTag = null, bool EnableRangeProcessing = false) : IStep<Stream, IResult>;
        public record File(string Path, string? ContentType = null, string? FileDownloadName = null, DateTimeOffset? LastModified = null, EntityTagHeaderValue? EntityTag = null, bool EnableRangeProcessing = false) : IStep<File, IResult>;
        public record Redirect(string Url, bool Permanent = false, bool PreserveMethod = false) : IStep<Redirect, IResult>;
        public record LocalRedirect(string LocalUrl, bool Permanent = false, bool PreserveMethod = false) : IStep<LocalRedirect, IResult>;
        public record RedirectToRoute(string? RouteName = null, object? RouteValues = null, bool Permanent = false, bool PreserveMethod = false, string? Fragment = null) : IStep<RedirectToRoute, IResult>;
        public record StatusCode(int Value) : IStep<StatusCode, IResult>;        
        public record Unauthorized() : IStep<Unauthorized, IResult>;
        public record BadRequest(object? Error = null) : IStep<BadRequest, IResult>;
        public record Conflict(object? Error = null) : IStep<Conflict, IResult>;
        public record UnprocessableEntity(object? Error = null) : IStep<UnprocessableEntity, IResult>;
        public record Problem(ProblemDetails ProblemDetails) : IStep<Problem, IResult>;
        public record ValidationProblem(IDictionary<string, string[]> Errors, string? Detail = null, string? Instance = null, int? StatusCode = null, string? Title = null, string? Type = null, IDictionary<string, object?>? Extensions = null) : IStep<ValidationProblem, IResult>;
        public record Created(string Uri, object? Value) : IStep<Created, IResult>;
        public record CreatedAtRoute(string? RouteName = null, object? RouteValues = null, object? Value = null) : IStep<CreatedAtRoute, IResult>;
        public record Accepted(string? Uri = null, object? Value = null) : IStep<Accepted, IResult>;
        public record AcceptedAtRoute(string? RouteName = null, object? RouteValues = null, object? Value = null) : IStep<AcceptedAtRoute, IResult>;

        internal class Runner:
            IRun<Ok, IResult>,
            IRun<NotFound, IResult>,
            IRun<NoContent, IResult>,
            IRun<Challenge, IResult>,
            IRun<Forbid, IResult>,
            IRun<SignIn, IResult>,
            IRun<SignOut, IResult>,
            IRun<Text, IResult>,
            IRun<Json, IResult>,
            IRun<Bytes, IResult>,
            IRun<Stream, IResult>,
            IRun<File, IResult>,
            IRun<Redirect, IResult>,
            IRun<LocalRedirect, IResult>,
            IRun<RedirectToRoute, IResult>,
            IRun<StatusCode, IResult>,
            IRun<Unauthorized, IResult>,
            IRun<BadRequest, IResult>,
            IRun<Conflict, IResult>,
            IRun<UnprocessableEntity, IResult>,
            IRun<Problem, IResult>,
            IRun<ValidationProblem, IResult>,
            IRun<Created, IResult>,
            IRun<CreatedAtRoute, IResult>,
            IRun<Accepted, IResult>,
            IRun<AcceptedAtRoute, IResult>
        {
            public IResult Run(Ok step) => Results.Ok(step.Value);
            public IResult Run(NotFound step) => Results.NotFound(step.Value);
            public IResult Run(NoContent _) => Results.NoContent();
            public IResult Run(Challenge step) => Results.Challenge(step.Properties, step.AuthenticationSchemes);
            public IResult Run(Forbid step) => Results.Forbid(step.Properties, step.AuthenticationSchemes);
            public IResult Run(SignIn step) => Results.SignIn(step.Principal, step.Properties, step.AuthenticationScheme);
            public IResult Run(SignOut step) => Results.SignOut(step.Properties, step.AuthenticationSchemes);
            public IResult Run(Text step) => Results.Text(step.Content, step.ContentType, step.ContentEncoding);
            public IResult Run(Json step) => Results.Json(step.Data, step.Options, step.ContentType, step.StatusCode);
            
            public IResult Run(Bytes step) => Results.Bytes(
                step.Contents,
                step.ContentType,
                step.FileDownloadName,
                step.EnableRangeProcessing,
                step.LastModified,
                step.EntityTag);

            public IResult Run(Stream step) => Results.Stream(
                step.Contents,
                step.ContentType,
                step.FileDownloadName,
                step.LastModified,
                step.EntityTag,
                step.EnableRangeProcessing);

            public IResult Run(File step) => Results.File(
                step.Path,
                step.ContentType,
                step.FileDownloadName,
                step.LastModified,
                step.EntityTag,
                step.EnableRangeProcessing);

            public IResult Run(Redirect step) => Results.Redirect(step.Url, step.Permanent, step.PreserveMethod);
            public IResult Run(LocalRedirect step) => Results.LocalRedirect(step.LocalUrl, step.Permanent, step.PreserveMethod);
            public IResult Run(RedirectToRoute step) => Results.RedirectToRoute(step.RouteName, step.RouteValues, step.Permanent, step.PreserveMethod, step.Fragment);
            public IResult Run(StatusCode step) => Results.StatusCode(step.Value);
            public IResult Run(Unauthorized step) => Results.Unauthorized();
            public IResult Run(BadRequest step) => Results.BadRequest(step.Error);
            public IResult Run(Conflict step) => Results.Conflict(step.Error);
            public IResult Run(UnprocessableEntity step) => Results.UnprocessableEntity(step.Error);
            public IResult Run(Problem step) => Results.Problem(step.ProblemDetails);
            public IResult Run(ValidationProblem step) => Results.ValidationProblem(
                step.Errors,
                step.Detail,
                step.Instance,
                step.StatusCode,
                step.Title,
                step.Type,
                step.Extensions);

            public IResult Run(Created step) => Results.Created(step.Uri, step.Value);
            public IResult Run(CreatedAtRoute step) => Results.CreatedAtRoute(step.RouteName, step.RouteValues, step.Value);
            public IResult Run(Accepted step) => Results.Accepted(step.Uri, step.Value);
            public IResult Run(AcceptedAtRoute step) => Results.AcceptedAtRoute(step.RouteName, step.RouteValues, step.Value);
        }
    }

    
}
#endif