using PasswordManager.Bot.Models;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PasswordManager.Bot.Services.Abstractions {
	// TODO: Add method to send message to BotUser: bot.SendMessage(user)
	/// <summary>
	/// Telegram Bot Service
	/// </summary>
	public interface IBot {
		public TelegramBotClient Client { get; }

		public Task<bool> SendMessageToAllAdmins(string message, ParseMode parseMode = ParseMode.Markdown);

		public bool IsTokenCorrect(string token);

		/// <summary>
		/// Tries to delete message. If unsuccessfully - edit it.
		/// </summary>
		public Task TryDeleteMessageAsync(ChatId chatId, int messageId);

		bool IsAdmin(int botUserId);
		bool IsAdmin(BotUser botUser);
	}
}

