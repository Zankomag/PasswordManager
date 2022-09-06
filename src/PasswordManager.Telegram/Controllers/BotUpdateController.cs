using Microsoft.AspNetCore.Mvc;
using PasswordManager.Telegram.Services.Abstractions;
using Telegram.Bot.Types;

namespace PasswordManager.Telegram.Controllers; 

[Route("api/bots")]
[ApiController]
public class BotUpdateController : ControllerBase {
	private readonly ITelegramBotHandler telegramBotHandler;
	//todo add feature to work with several bots damn
	private readonly IBot bot;

	public BotUpdateController(ITelegramBotHandler telegramBotHandler, IBot bot) {
		this.telegramBotHandler = telegramBotHandler;
		this.bot = bot;
	}

	[HttpPost("update/{token}")]
	public IActionResult Post([FromBody]Update update, string token) {
		if (bot.IsTokenCorrect(token)) {
			telegramBotHandler.HandleUpdateAsync(update);
		}
		return Ok();
	}
}