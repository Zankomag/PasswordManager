using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using User = PasswordManager.Core.Entities.User;

namespace PasswordManager.Bot.Commands {
	public class DeleteMessageCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, User user) {
			await BotHandler.TryDeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
		}
	}
}
