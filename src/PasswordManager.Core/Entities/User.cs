using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PasswordManager.Core.Entities {

	//This should be in separate database of telegram bot
	//but it's here until service become larger
	public enum UserAction {

		/// <summary>
		/// Default action
		/// </summary>
		Search = 0,
		AssembleAccount = 1,
		UpdateAccount = 2,
		SetUpPasswordGeneratorLength = 3,
		EnterDecryptionKey = 4,
		UpdateUserSettings = 5,

		/// <summary>
		///     Used to Re-encrypt or encrypt unencrypted existing passwords
		///     Encryption during account assembling is handled by AddAccount action command
		/// </summary>
		EncryptPassword = 6

	}


	public class User {

		public int Id { get; set; }

		/// <summary>
		///     Language code
		/// </summary>
		[Required] public string Lang { get; set; }

		// TODO rename property to PasswordGeneratorPattern, but make mapping in entity framawork 
		/// <summary>
		///     Generator pattern
		/// </summary>
		[Required] public string GenPattern { get; set; }

		[Required] public UserAction Action { get; set; }

		/// <summary>
		///     Hint for encryption key in case user don't remember it
		///     User manually adds his hint
		/// </summary>
		[StringLength(128)] public string KeyHint { get; set; }

		/// <summary>
		///     Time period after which user get notification that he needs to update his password
		///     If equals to 0 than outdated checking is disabled
		///     Can be overridden individually for each account
		///     Default is 6 months
		/// </summary>
		public TimeSpan? OutdatedTime { get; set; }


		public virtual List<Account> Accounts { get; set; }

	}

}