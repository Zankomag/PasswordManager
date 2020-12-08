using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Abstractions {

	/// <summary>
	/// Telegram Bot Service
	/// </summary>
	public interface IBotService {
		public TelegramBotClient Client { get; }

		/// <summary>
		/// List of admin ids
		/// </summary>
		public int[] Admins { get; }

		public Task<bool> SendMessageToAllAdmins(string message);

		public bool IsTokenCorrect(string token);

		/// <summary>
		/// Tries to delete message. If unsuccessfully - edit it.
		/// </summary>
		public Task TryDeleteMessageAsync(ChatId chatId, int messageId);

	}
}

