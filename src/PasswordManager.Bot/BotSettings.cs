
namespace PasswordManager.Bot {

	/// <summary>
	/// Telegram Bot Settings
	/// </summary>
	public class BotSettings {
		public string Token { get; set; }
		/// <summary>
		/// Domain which Telegram will send webhook requests to
		/// </summary>
		public int[] AdminIds { get; set; }
		public string Domain { get; set; }

	}
}
