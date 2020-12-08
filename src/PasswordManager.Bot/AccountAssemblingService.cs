using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Bot.Models;
using PasswordManager.Core.Entities;

namespace PasswordManager.Bot {
	public class AccountAssemblingService : IAccountAssemblingService {
		//Assembling Accounts data is stored in memory
		//because storing it in database doesn't worth it
		private readonly Dictionary<int, AccountAssembleModel> assemblingAccounts;

		public AccountAssemblingService() {
			assemblingAccounts = new Dictionary<int, AccountAssembleModel>();
		}

		public void Cancel(int userId) => assemblingAccounts.Remove(userId);
		public Account Release(int userId) {
			non implemented
		}

		private bool isAssembled(AccountAssembleModel) {
			//TODO:
			//Check all needed fields to be assembled
		}

		//TODO:
		//Add enum for assembling property and 
		//return it to AddAccount command to switch
		//AddAccountCommand should have NextStep switch case along with above
		//So Assemble Account Service should bother about assembling steps
	}
}
