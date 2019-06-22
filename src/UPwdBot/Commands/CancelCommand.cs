using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace UPwdBot.Commands {
	public class CancelCommand : IMessageCommand {
		public async Task ExecuteAsync(Message message, string langCode) {
			if (BotHandler.AssemblingAccounts.ContainsKey(message.From.Id)){
				BotHandler.AssemblingAccounts.Remove(message.From.Id);
				await Bot.Instance.Client.SendTextMessageAsync(message.From.Id,
					String.Format(Localization.GetMessage("Cancel", langCode),"Add"));
			} else {
				await Bot.Instance.Client.SendTextMessageAsync(message.From.Id,
					Localization.GetMessage("NoCancel", langCode));
			}
		}
	}
}
