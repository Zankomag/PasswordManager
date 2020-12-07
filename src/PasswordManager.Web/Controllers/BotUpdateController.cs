using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using PasswordManager.Bot.Abstractions;

namespace PasswordManager.Bot.Controllers
{
    [Route("api/bots")]
    [ApiController]
    public class BotUpdateController : ControllerBase {

		private IBotHandler botHandler;
		private IBotService botService;

		public BotUpdateController(IBotHandler botHandler, IBotService botService) {
			this.botHandler = botHandler;
			this.botService = botService;
		}

		[HttpPost("{token}")]
		public IActionResult Post([FromBody]Update update, string token) {
			if (botService.IsTokenCorrect(token)) {
				botHandler.HandleUpdate(update);
			}
			return Ok();
		}
	}
}