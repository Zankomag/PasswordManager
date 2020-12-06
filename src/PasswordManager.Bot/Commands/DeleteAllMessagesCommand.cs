using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using User = PasswordManager.Core.Entities.User;


namespace PasswordManager.Bot.Commands {
	public class DeleteAllMessagesCommand : IMessageCommand {
		//
		//===========================================================================
		//EXPERIMENTAL FEATURE (IN DEVELOPMENT)
		//==========================================================================
		//
		public async Task ExecuteAsync(Message message, User user) {
			//CYCLE ALL MESSAGES THAT NEEDS TO BE DELETED 
			try {
				await BotService.Instance.Client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
			}
			catch (Telegram.Bot.Exceptions.ApiRequestException) {
				//
				//TODO:
				//
				//Try to edit:
				//await Bot.Instance.Client.EditMessageTextAsync(chatId, messageId, "🗑️");
				//if catch exception than do nothing
			}
		}


	}
}
