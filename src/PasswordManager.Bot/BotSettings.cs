using Telegram.Bot.Types;

namespace PasswordManager.Bot {

	public class BotSettings {
		public string BotToken { get; set; }
		public ChatId AdminId { get; set; }
		public string ConnectionString { get; set; }
		public string Domain { get; set; }

	}
}
