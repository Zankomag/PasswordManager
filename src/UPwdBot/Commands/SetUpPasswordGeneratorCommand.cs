using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using UPwdBot.Types;

namespace UPwdBot.Commands {
	public class SetUpPasswordGeneratorCommand : IMessageCommand, ICallBackQueryCommand {
		public async Task ExecuteAsync(Message message, Types.User user) {

		}
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {

		}
	}
}
