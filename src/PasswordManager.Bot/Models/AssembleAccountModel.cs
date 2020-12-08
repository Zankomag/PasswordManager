using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PasswordManager.Bot.Models {
	public class AssembleAccountModel {
		public long Id { get; set; }
		public int UserId { get; set; }
		[StringLength(60)] public string AccountName { get; set; }
		[StringLength(70)] public string Link { get; set; }
		[StringLength(40)] public string Login { get; set; }
		[StringLength(8096)] public string Password { get; set; }
		[StringLength(1024)] public string Note { get; set; }
		public bool Encrypted { get; set; }

		public bool SkipLink { get; set; }
	}
}
