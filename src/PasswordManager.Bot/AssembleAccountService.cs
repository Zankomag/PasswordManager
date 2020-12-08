using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Bot.Models;
using PasswordManager.Core.Entities;

namespace PasswordManager.Bot {
	public class AssembleAccountService : IAssembleAccountService {
		//Assembling Accounts data is stored in memory
		//because storing it in database doesn't worth it
		private readonly Dictionary<int, AssembleAccountModel> assemblingAccounts = new Dictionary<int, AssembleAccountModel>();

		public void Cancel(int userId) => assemblingAccounts.Remove(userId);
		public Account Release(int userId) {
			non implemented
		}

		private bool isAssembled(AssembleAccountModel) {
			//TODO:
			//Check all needed fields to be assembled
		}

		//TODO:
		//Add enum for assembling property and 
		//return it to AddAccount command to switch
	}
}
