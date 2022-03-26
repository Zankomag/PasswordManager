using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class DeleteMessageCommand : Abstractions.BotCommand, ICallbackQueryCommand {

		public DeleteMessageCommand(IBot bot) : base(bot) { }

		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser botUser) 
			=> await Bot.TryDeleteMessageAsync(
				callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId);
		
	}
}
