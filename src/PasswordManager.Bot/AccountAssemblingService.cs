using System;
using System.Collections.Generic;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Bot.Enums;
using PasswordManager.Bot.Models;
using PasswordManager.Core.Entities;

namespace PasswordManager.Bot {
	public class AccountAssemblingService : IAccountAssemblingService {
		//Assembling Accounts data is stored in memory
		//because storing it in database doesn't worth it
		private readonly Dictionary<int, AccountAssemblingModel> assemblingAccounts;

		public AccountAssemblingService() {
			assemblingAccounts = new Dictionary<int, AccountAssemblingModel>();
		}

		public void Cancel(int userId) => assemblingAccounts.Remove(userId);
		public AccountAssemblingStage GetCurrentStage(int userId) {
			if(assemblingAccounts.TryGetValue(userId, out AccountAssemblingModel accountAssembleModel)) {
				return accountAssembleModel.AccountAssemblingStage;
			}
			return AccountAssemblingStage.None;
		}

		public AccountAssemblingStage GetNextStage(int userId) {
			if (assemblingAccounts.TryGetValue(userId, out AccountAssemblingModel accountAssembleModel)) {
				if ((int)accountAssembleModel.AccountAssemblingStage > (int)AccountAssemblingStage.Release)
					throw new InvalidOperationException();
				return accountAssembleModel.AccountAssemblingStage++;
			}
			return AccountAssemblingStage.None;
		}

		public Account Release(int userId) {
			if (assemblingAccounts.TryGetValue(userId, out AccountAssemblingModel accountAssembleModel)) {
				if(accountAssembleModel.AccountAssemblingStage == AccountAssemblingStage.Release) {
					assemblingAccounts.Remove(userId);
					return new Account {
						Id = accountAssembleModel.Id,
						AccountName = accountAssembleModel.AccountName,
						UserId = accountAssembleModel.UserId,
						Link = accountAssembleModel.Link,
						Note = accountAssembleModel.Note,
						Login = accountAssembleModel.Login,
						Password = accountAssembleModel.Password,
						Encrypted = accountAssembleModel.Encrypted,
						OutdatedTime = new TimeSpan(0, 0, 0),
						PasswordUpdatedDate = DateTime.Now,
					};
				}
			}
			return null;
		}

		//TODO:
		//Add enum for assembling property and 
		//return it to AddAccount command to switch
		//AddAccountCommand should have NextStep switch case along with above
		//So Assemble Account Service should bother about assembling steps
		//
		//TODO:
		//After user enters password (or accepts it via button)
		//It has option to enter EncryptionKey 
		//There is invintation to enter key message that holds 2 buttons:
		//first button allows to skip password encryption
		//second button shows Encryption key hint
	}
}
