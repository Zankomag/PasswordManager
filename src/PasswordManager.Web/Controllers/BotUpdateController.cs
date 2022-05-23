using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using PasswordManager.Bot.Services.Abstractions;

namespace PasswordManager.Web.Controllers; 

[Route("api/bots")]
[ApiController]
public class BotUpdateController : ControllerBase {
	//TODO Move Bot Controller to Bot project!
	//Create helper class that maps controller from other assembly
	private readonly ITelegramBotHandler telegramBotHandler;
	private readonly IBot bot;

	public BotUpdateController(ITelegramBotHandler telegramBotHandler, IBot bot) {
		this.telegramBotHandler = telegramBotHandler;
		this.bot = bot;
	}

	[HttpPost("{token}")]
	public IActionResult Post([FromBody]Update update, string token) {
		if (bot.IsTokenCorrect(token)) {
			telegramBotHandler.HandleUpdateAsync(update);
		}
		return Ok();
	}
}