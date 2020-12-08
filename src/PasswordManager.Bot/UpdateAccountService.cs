using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Bot.Types;

namespace PasswordManager.Bot {
	public class UpdateAccountService : IUpdateAccountService {
		//Updating Accounts data is stored in memory
		//because storing it in database doesn't worth it
		private Dictionary<int, AccountUpdate> updatingAccounts { get; set; } = new Dictionary<int, AccountUpdate>();
	}
}
