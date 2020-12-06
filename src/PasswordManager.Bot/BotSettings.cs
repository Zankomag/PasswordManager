
namespace PasswordManager.Bot {

	/// <summary>
	/// Telegram Bot Settings
	/// </summary>
	public class BotSettings {
		public string Token { get; set; }
		public int[] AdminIds { get; set; }
		/// <summary>
		/// Domain which Telegram will send webhook requests to
		/// </summary>
		public string Domain { get; set; }

	}
}
