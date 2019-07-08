using System.Threading.Tasks;
using Telegram.Bot.Types;
using Uten.Localization.MultiUser;

namespace UPwdBot.Commands {
	public class HelpCommand : IMessageCommand {
		public async Task ExecuteAsync(Message message, string langCode) {
			await BotHandler.Bot.SendTextMessageAsync(message.From.Id, Localization.GetMessage("Help", langCode));
		}
	}
}
