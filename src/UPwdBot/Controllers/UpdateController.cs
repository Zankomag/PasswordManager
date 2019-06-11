using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace UPwdBot.Controllers
{
    [Route("api/bots/upwdbot")]
    [ApiController]
    public class UpdateController : ControllerBase {

		[HttpPost]
		public IActionResult Post([FromBody]Update update) {
			BotHandler.Instance.HandleUpdate(update);
			return Ok();
		}
	}
}