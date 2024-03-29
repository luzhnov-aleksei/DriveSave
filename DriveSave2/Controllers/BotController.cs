﻿using DriveSave2.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Services;
using Telegram.Bot.Types;

namespace Telegram.Bot.Controllers;

public class BotController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] UpdateHandlers handleUpdateService,
        CancellationToken cancellationToken)
    {
        await handleUpdateService.HandleUpdateAsync(update, cancellationToken);
        return Ok();
    }
}