using PasswordManager.Bot.Models;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PasswordManager.Bot.Services.Abstractions {
	// TODO: Add method to send message to BotUser: bot.SendMessage(user)
	//TODO: inherit from TelegramBotClient and get rid of Client field
	/// <summary>
	/// Telegram Bot Service
	/// </summary>
	public interface IBot {
		public TelegramBotClient Client { get; }

		/// <summary>
		/// Indicates whether bot is accessible to everyone 
		/// 
		/// <para>If bot is public, he will register every new user. 
		/// If bot is private new users registration should be restricted.
		/// For example, new users can be registered manually by admins
		/// or new user must enter registration code he received somewhere else.</para>
		/// 
		/// <para>If bot is considered as private, it's up to you to decide how he will 
		/// handle requests from unregistered users. He can just ignore them pretending he is dead, 
		/// he can answer that he works only for registered users, he can send admin contact or whatever.</para>
		/// </summary>
		public bool IsPublic { get; }

		public Task<bool> SendMessageToAllAdmins(string message, ParseMode parseMode = ParseMode.Markdown);

		public bool IsTokenCorrect(string token);

		/// <summary>
		/// Tries to delete message. If unsuccessfully - edit it.
		/// </summary>
		public Task TryDeleteMessageAsync(ChatId chatId, int messageId);

		bool IsAdmin(long botUserId);
		bool IsAdmin(BotUser botUser);
	}
}

