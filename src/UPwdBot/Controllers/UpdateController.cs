using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace UPwdBot.Controllers
{
    [Route("api/bots/upwdbot")]
    [ApiController]
    public class UpdateController : ControllerBase {

		TelegramBotClient bot = Bot.Instance.Client;

		[HttpPost]
		public async Task<IActionResult> Post([FromBody]Update update) {
			await bot.SendTextMessageAsync(update.Message.Chat.Id, "GOT IT");
			//await updateService.ManageRequest(update);
			return Ok();
		}
	}
}