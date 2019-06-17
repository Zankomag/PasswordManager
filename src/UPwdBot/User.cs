
namespace UPwdBot {
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
	}
}
