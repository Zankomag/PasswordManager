using System.Threading.Tasks;
using Telegram.Bot.Types;
using Uten.Localization.MultiUser;

namespace UPwdBot.Commands {
	public class HelpCommand : IMessageCommand {
		public async Task ExecuteAsync(Message message, Types.User user) {
			await BotHandler.Bot.SendTextMessageAsync(message.From.Id, Localization.GetMessage("Help", user.Lang));
		}
	}
}
