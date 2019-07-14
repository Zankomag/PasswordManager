using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace UPwdBot.Commands {
	public class DeleteAllMessagesCommand : IMessageCommand {
		public async Task ExecuteAsync(Message message, Types.User user) {
			try {
				await Bot.Instance.Client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
			}
			catch (Telegram.Bot.Exceptions.ApiRequestException) {
				//await Bot.Instance.Client.EditMessageTextAsync(chatId, messageId, "🗑️");
				await Bot.Instance.Client.SendTextMessageAsync(message.Chat.Id, "Cannot delete message " + message.MessageId);
			}
		}


	}
}
