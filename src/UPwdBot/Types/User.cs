using UPwdBot.Types.Enums;

namespace UPwdBot.Types {

	public class User {
		public const UserAction DefaultAction = UserAction.Search;

		public int Id { get; set; }
		/// <summary>
		/// Language code
		/// </summary>
		public string Lang { get; set; }
		public string Password { get; set; }
		/// <summary>
		/// Generator pattern
		/// </summary>
		public string GenPattern { get; set; }

		public byte Action {
			get => (byte)ActionType;
			set => ActionType = (UserAction)value;
		}

		public UserAction ActionType { get; set; }

	}
}
