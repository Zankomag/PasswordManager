using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Core.Entities;

namespace PasswordManager.Bot {
	public class AssembleAccountService : IAssembleAccountService {
		//Updating Accounts data is stored in memory
		//because storing it in database doesn't worth it
		private Dictionary<int, Account> assemblingAccounts { get; set; } = new Dictionary<int, Account>();
	}
}
