
namespace UPwdBot.Types {

	public enum Actions : byte {
		Search = 0,
		Assemble,
		Update
	}

	public enum Updates : byte {
		Password = 80,	//'P' code
		AccountName = 78,		//'N' code
		Link = 82,		//'R' code
		Login = 76		//'L' code
	}

	public class User {
		public const Actions DefaultAction = Actions.Search;

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
			set => ActionType = (Actions)value;
		}

		public Actions ActionType { get; set; }

	}
}
