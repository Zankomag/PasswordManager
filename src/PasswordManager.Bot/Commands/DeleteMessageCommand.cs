using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;

namespace PasswordManager.Bot.Commands {
	public class DeleteMessageCommand : ICallbackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			await BotHandlerService.TryDeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
		}
	}
}
