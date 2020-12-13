using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Bot;
using PasswordManager.Bot.Models;

namespace PasswordManager.Bot.Services {
	public class AccountUpdatingService : IAccountUpdatingService {
		//Updating Accounts data is stored in memory
		//because storing it in database doesn't worth it
		private readonly Dictionary<int, AccountUpdatingModel> updatingAccounts;

		public AccountUpdatingService() {
			updatingAccounts = new Dictionary<int, AccountUpdatingModel>();
		}
	}
}
