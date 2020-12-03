using System.ComponentModel.DataAnnotations;
using System;

namespace PasswordManager.Core.Entities {
	public class Account {
		public long Id { get; set; }
		public int UserId { get; set; }
		[Required] public DateTime PasswordUpdatedDate { get; set; }

		[Required] [StringLength(60)] public string AccountName { get; set; }
		[StringLength(70)] public string Link { get; set; }
		[StringLength(40)] public string Login { get; set; }
		[StringLength(2048)] public string Password { get; set; }
		[StringLength(512)] public string Note { get; set; }
		/// <summary>
		/// Indicates whether password is encrypted
		/// </summary>
		[Required] public bool Encrypted { get; set; }
		/// <summary>
		/// Time period after which user get notification that he needs to update his password
		/// If equals to 0 than outdated checking is disabled 
		/// This field overrides global field in User
		/// Default is null
		/// </summary>
		public TimeSpan OutdatedTime { get; set; }


		public virtual User User { get; set; }
	}
}
