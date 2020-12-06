using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Controllers
{
    [Route("api/bots")]
    [ApiController]
    public class UpdateController : ControllerBase {

		[HttpPost("{token}")]
		public IActionResult Post([FromBody]Update update, string token) {
			if (BotService.Instance.IsTokenCorrect(token)) {
				BotHandlerService.Instance.HandleUpdate(update);
			}
			return Ok();
		}
	}
}