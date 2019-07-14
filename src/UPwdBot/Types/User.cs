
namespace UPwdBot.Types {

	public enum Actions : byte {
		Search = 0,
		Assemble,
		Update
	}

	public class User {
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
