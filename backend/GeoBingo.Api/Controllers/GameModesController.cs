using System.Collections.Generic;
using System.Linq;
using GeoBingo.Api.Mapping;
using GeoBingo.Contracts.GameModes;
using GeoBingo.GameModes.Registry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GeoBingo.Api.Controllers;

[ApiController]
[Route("api/game-modes")]
public sealed class GameModesController(
    IGameModeRegistry gameModeRegistry,
    GameModeMapper gameModeMapper
) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [EndpointName("ListGameModes")]
    [EndpointSummary("List registered game modes")]
    [EndpointDescription("Returns the registered game modes as ordered public metadata without exposing active lobby or round state.")]
    [ProducesResponseType<IReadOnlyList<GameModeSummary>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<GameModeSummary>> GetGameModes()
    {
        var gameModes = gameModeRegistry.Modes
            .Select(gameModeMapper.Map)
            .ToArray();
        return Ok(gameModes);
    }
}
