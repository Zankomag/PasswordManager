using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace UPwdBot.Commands {
	public class DeleteMessageCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			await DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
		}

		/// <summary>
		/// Tries to delete message. If unsuccessfully - edit it.
		/// </summary>
		public static async Task DeleteMessageAsync(ChatId chatId, int messageId) {
			try {
				await Bot.Instance.Client.DeleteMessageAsync(chatId, messageId);
			}
			catch (Telegram.Bot.Exceptions.ApiRequestException) {
				await Bot.Instance.Client.EditMessageTextAsync(chatId, messageId, "🗑️");
			}
		}
	}
}
