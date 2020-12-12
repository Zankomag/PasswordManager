using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class DeleteMessageCommand : Abstractions.BotCommand, ICallbackQueryCommand {

		public DeleteMessageCommand(IBot botService) : base(botService) { }

		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			await BotHandler.TryDeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
		}
	}
}
