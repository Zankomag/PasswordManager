using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using PasswordManager.Bot.Services.Abstractions;

namespace PasswordManager.Web.Controllers
{
    [Route("api/bots")]
    [ApiController]
    public class BotUpdateController : ControllerBase {

		private readonly IBotHandler botHandler;
		private readonly IBot bot;

		public BotUpdateController(IBotHandler botHandler, IBot bot) {
			this.botHandler = botHandler;
			this.bot = bot;
		}

		[HttpPost("{token}")]
		public IActionResult Post([FromBody]Update update, string token) {
			if (bot.IsTokenCorrect(token)) {
				botHandler.HandleUpdate(update);
			}
			return Ok();
		}
	}
}