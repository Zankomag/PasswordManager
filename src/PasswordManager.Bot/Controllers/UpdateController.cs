using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Controllers
{
    [Route("api/bots")]
    [ApiController]
    public class UpdateController : ControllerBase {

		[HttpPost("{token}")]
		public IActionResult Post([FromBody]Update update, string token) {
			if (Bot.Instance.IsTokenCorrect(token)) {
				BotHandler.Instance.HandleUpdate(update);
			}
			return Ok();
		}
	}
}