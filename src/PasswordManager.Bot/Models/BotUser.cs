using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Models {
	public class BotUser {
		public int Id { get; set; }
		/// <summary>
		/// Language code
		/// </summary>
		public string Lang { get; set; }
		public UserAction Action { get; set; }


		public static implicit operator BotUser(User user) {
			if (user == null) 
				return null;
			return new BotUser {
				Id = user.Id,
				Lang = user.Lang,
				Action = user.Action
			};
		}
	}
}
