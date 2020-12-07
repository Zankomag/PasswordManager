using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Models {
	public class BotUser {
		public int Id { get; set; }
		/// <summary>
		/// Language code
		/// </summary>
		public string Lang { get; set; }
		public UserAction Action { get; set; }
	}
}
