

namespace UPwdBot {
	public class Account {
		public int Id { get; set; }
		public int UserId { get; set; }
		public string AccountName { get; set; }
		public string Link { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }

		public bool SkipLink { get; set; }
	}
}
