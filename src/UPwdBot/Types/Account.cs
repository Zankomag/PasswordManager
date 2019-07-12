

namespace UPwdBot.Types {
	public class Account {

		public const int maxAccountNameLength = 50;
		public const int maxLinkLength = 54; //AccountName + ".com"
		public const int maxLoginLength = 35;
		public const int maxPasswordLength = 63;

		public long Id { get; set; }
		public int UserId { get; set; }
		public string AccountName { get; set; }
		public string Link { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }

		public bool SkipLink { get; set; }
	}
}
