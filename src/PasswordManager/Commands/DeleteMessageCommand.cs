using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Commands {
	public class DeleteMessageCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			await BotHandler.TryDeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
		}
	}
}
