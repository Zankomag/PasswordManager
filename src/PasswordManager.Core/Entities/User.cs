using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PasswordManager.Core.Entities {
	public class User {

		public enum UserAction {
			Search = 0,
			Assemble = 1,
			Update = 2,
			UpdatePasswordLength = 3
		}

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
		public string KeyHint { get; set; }


		public virtual List<Account> Accounts { get; set; }

	}
}
