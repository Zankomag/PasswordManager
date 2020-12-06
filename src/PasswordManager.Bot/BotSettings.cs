using Telegram.Bot.Types;

namespace PasswordManager.Bot {

	/// <summary>
	/// Telegram Bot Settings
	/// </summary>
	public class BotSettings {
		public string BotToken { get; set; }
		public int[] AdminIds { get; set; }
		public string Domain { get; set; }

	}
}
