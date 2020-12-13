using System.Collections.Generic;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Enums;

namespace PasswordManager.Bot.Services {
	public class AccountUpdatingService : IAccountUpdatingService {
		//Updating Accounts data is stored in memory
		//because storing it in database doesn't worth it
		private readonly Dictionary<long, AccountUpdatingModel> updatingAccounts;

		public AccountUpdatingService() {
			updatingAccounts = new Dictionary<long, AccountUpdatingModel>();
		}

		public void StartUpdatingRequest(int userId, long accountId, AccountUpdatingType accountUpdatingType)
			=> updatingAccounts[userId] = new AccountUpdatingModel(accountUpdatingType, accountId);
		
		public void FinishUpdatingRequest(int userId)
			=> updatingAccounts.Remove(userId);

		public AccountUpdatingModel GetAccountUpdatingModel(int userId) {
			if (updatingAccounts.TryGetValue(userId, out AccountUpdatingModel account))
				return account;
			return null;
		}
	}
}
