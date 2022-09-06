using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Telegram.Commands.Abstractions;
using PasswordManager.Telegram.Models;
using PasswordManager.Telegram.Services.Abstractions;

namespace PasswordManager.Telegram.Commands; 

public class DeleteMessageCommand : Abstractions.BotCommand, ICallbackQueryCommand {

	public DeleteMessageCommand(IBot bot) : base(bot) { }

	async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser botUser) 
		=> await Bot.TryDeleteMessageAsync(
			callbackQuery.Message.Chat.Id,
			callbackQuery.Message.MessageId);
		
}