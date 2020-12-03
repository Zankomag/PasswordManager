﻿using System.ComponentModel.DataAnnotations;
using System;

namespace PasswordManager.Core.Entities {
	public class Account {
		public long Id { get; set; }
		public int UserId { get; set; }
		[Required] public DateTime PasswordUpdatedDate { get; set; }
		[Required] public string AccountName { get; set; }
		public string Link { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
		public string Note { get; set; }
		/// <summary>
		/// Indicates whether password is encrypted
		/// </summary>
		[Required] public bool Encrypted { get; set; }

		
		public virtual User User { get; set; }
	}
}