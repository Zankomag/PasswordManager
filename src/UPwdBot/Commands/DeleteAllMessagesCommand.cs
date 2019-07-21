using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace UPwdBot.Commands {
	public class DeleteAllMessagesCommand : IMessageCommand {
		//
		//===========================================================================
		//EXPERIMENTAL FEATURE (IN DEVELOPMENT)
		//==========================================================================
		//
		public async Task ExecuteAsync(Message message, Types.User user) {
			//CYCLE ALL MESSAGES THAT NEEDS TO BE DELETED 
			try {
				await Bot.Instance.Client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
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
