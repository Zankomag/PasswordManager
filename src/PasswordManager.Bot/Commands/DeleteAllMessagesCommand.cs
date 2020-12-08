using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Application.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	//public class DeleteAllMessagesCommand : Abstractions.BotCommand, IMessageCommand {

	//	public DeleteAllMessagesCommand(IBotService botService) : base(botService) { }
	//	//
	//	//===========================================================================
	//	//EXPERIMENTAL FEATURE
	//	//==========================================================================
	//	//
	//	public async Task ExecuteAsync(Message message, BotUser user) {
	//		//CYCLE ALL MESSAGES THAT NEEDS TO BE DELETED 
	//		try {
	//			await botService.Client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
	//		}
	//		catch (Telegram.Bot.Exceptions.ApiRequestException) {
	//			//
	//			//TODO:
	//			//
	//			//Try to edit:
	//			//await Bot.Instance.Client.EditMessageTextAsync(chatId, messageId, "🗑️");
	//			//if catch exception than do nothing
	//		}
	//	}


	//}
}
