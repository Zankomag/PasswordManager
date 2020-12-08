using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace PasswordManager.Core.Entities {

	public enum UserAction {
		Search = 0,
		AssembleAccount = 1,
		Update = 2,
		UpdatePasswordLength = 3,
		EnterKey = 4
	}

	public class User {
		public int Id { get; set; }
		/// <summary>
		/// Language code
		/// </summary>
		[Required] public string Lang { get; set; }
		/// <summary>
		/// Generator pattern
		/// </summary>
		[Required] public string GenPattern { get; set; }
		[Required] public UserAction Action { get; set; }
		/// <summary>
		/// Hint for encryption key ib case user don't remember it
		/// User manually adds his hint
		/// </summary>
		[StringLength(128)] public string KeyHint { get; set; }
		/// <summary>
		/// Time period after which user get notification that he needs to update his password
		/// If equals to 0 than outdated checking is disabled
		/// Can be overrided individually for each account
		/// Default is 6 months
		/// </summary>
		public TimeSpan OutdatedTime { get; set; }


		public virtual List<Account> Accounts { get; set; }

	}
}
