using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using GeoBingo.Api.Authentication;
using GeoBingo.Api.Lobbies.Mapping;
using GeoBingo.Api.Lobbies.Runtime;
using GeoBingo.Contracts.Common;
using GeoBingo.Contracts.Lobbies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GeoBingo.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/lobbies")]
public sealed class LobbyController(
    ILobbyRegistry lobbyRegistry,
    LobbyRestMapper lobbyRestMapper
) : ControllerBase
{
    [HttpPost]
    [EndpointName("CreateLobby")]
    [EndpointSummary("Create an in-memory lobby")]
    [EndpointDescription("Creates a waiting lobby and makes the authenticated caller its host.")]
    [ProducesResponseType<CreateLobbyResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public ActionResult<CreateLobbyResponse> Create()
    {
        var identity = ReadIdentity();

        var runtime = lobbyRegistry.Create(
            new LobbyCreation(
                identity.UserId,
                identity.Handle,
                identity.DisplayName,
                identity.AvatarUrl,
                new LobbySettings()));
        var response = lobbyRestMapper.MapCreated(runtime.ReadSummary());

        return Created($"/api/lobbies/resolve/{response.Code}", response);
    }

    [HttpGet("resolve/{code}")]
    [EndpointName("ResolveLobbyCode")]
    [EndpointSummary("Resolve an active lobby code")]
    [EndpointDescription("Returns only minimal, current-user-specific joinability information.")]
    [ProducesResponseType<ResolveLobbyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResolveLobbyResponse>> Resolve(
        [FromRoute]
        [Required]
        [MinLength(8)]
        [MaxLength(8)]
        [RegularExpression("^[A-Z0-9]{8}$")]
        string code)
    {
        var identity = ReadIdentity();

        if (!LobbyRegistry.TryNormalizeCode(code, out var normalizedCode)
            || !lobbyRegistry.TryResolveCode(normalizedCode, out var lobbyId)
            || !lobbyRegistry.TryGet(lobbyId, out var runtime)
            || runtime is null)
        {
            throw new LobbyRuntimeException(
                ErrorCode.LOBBY_NOT_FOUND,
                "The lobby was not found.");
        }

        var response = await runtime.EnqueueReadAsync(
                (state, _) => ValueTask.FromResult(
                    lobbyRestMapper.MapResolved(
                        state,
                        identity.UserId,
                        lobbyRegistry.IsAcceptingCreations)),
                HttpContext.RequestAborted)
            .ConfigureAwait(false);
        return Ok(response);
    }

    private LobbyControllerIdentity ReadIdentity()
    {
        var handle = User.FindFirstValue(
            GeoBingoClaimTypes.Handle);
        var displayName = User.FindFirstValue(ClaimTypes.Name);
        var avatarUrl = User.FindFirstValue(
            GeoBingoClaimTypes.AvatarUrl);
        if (!Guid.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var userId)
            || userId == Guid.Empty
            || string.IsNullOrWhiteSpace(handle)
            || string.IsNullOrWhiteSpace(displayName))
        {
            throw new LobbyRuntimeException(
                ErrorCode.AUTH_REQUIRED,
                "The current session could not be validated.");
        }

        return new LobbyControllerIdentity(
            userId,
            handle,
            displayName,
            avatarUrl);
    }

    private sealed record LobbyControllerIdentity(
        Guid UserId,
        string Handle,
        string DisplayName,
        string? AvatarUrl);
}
