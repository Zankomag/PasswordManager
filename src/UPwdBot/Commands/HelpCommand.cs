using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace UPwdBot.Commands {
	public class HelpCommand : ILocalizedCommand {
		public async Task ExecuteAsync(Message message, string langCode) {
			await BotHandler.Bot.SendTextMessageAsync(message.From.Id, Localization.GetMessage("Help", langCode));
		}
	}
}
