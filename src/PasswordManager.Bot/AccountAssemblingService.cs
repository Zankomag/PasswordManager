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

		public AccountAssemblingStage Create(int userId) {
			AccountAssemblingStage nextAccountAssemblingStage = AccountAssemblingStage.AddAccountName;
			assemblingAccounts[userId] = new AccountAssemblingModel() {
				AccountAssemblingStage = nextAccountAssemblingStage,
				UserId = userId,
			};
			return nextAccountAssemblingStage;
		}
		//TODO:
		//Add EncryptionKey in the last arg line in inline assembling
		// /add AccountName \n Login => Ask for password? then for encryptionKey
		// /add AccountName \n Login \n Password => Ask for encryptionKey
		// /add AccountName \n Login \n Password \n EncryptionKey
		// /add AccountName \n Link \n Login \n Password \n EncryptionKey
		// /add AccountName \n Link \n Note \n Login \n Password \n EncryptionKey
		public AccountAssemblingStage Create(int userId, string[] args) => throw new NotImplementedException();

		public AccountAssemblingStage GetCurrentStage(int userId) {
			if(assemblingAccounts.TryGetValue(userId, out AccountAssemblingModel accountAssemblingModel)) {
				return accountAssemblingModel.AccountAssemblingStage;
			}
			return AccountAssemblingStage.None;
		}

		public AccountAssemblingStage GetNextStage(int userId) {
			if (assemblingAccounts.TryGetValue(userId, out AccountAssemblingModel accountAssemblingModel)) {
				if (accountAssemblingModel.AccountAssemblingStage == AccountAssemblingStage.Release)
					throw new InvalidOperationException("Account is already assembled");
				return accountAssemblingModel.AccountAssemblingStage + 1;
			}
			return AccountAssemblingStage.None;
		}

		public Account Release(int userId) {
			if (assemblingAccounts.TryGetValue(userId, out AccountAssemblingModel accountAssemblingModel)) {
				if(accountAssemblingModel.AccountAssemblingStage == AccountAssemblingStage.Release) {
					assemblingAccounts.Remove(userId);
					return new Account {
						AccountName = accountAssemblingModel.AccountName,
						UserId = accountAssemblingModel.UserId,
						Link = accountAssemblingModel.Link,
						Note = accountAssemblingModel.Note,
						Login = accountAssemblingModel.Login,
						Password = accountAssemblingModel.Password,
						Encrypted = accountAssemblingModel.Encrypted,
						OutdatedTime = new TimeSpan(0, 0, 0),
						PasswordUpdatedDate = DateTime.Now,
					};
				}
			}
			return null;
		}

		public AccountAssemblingStage Assemble(int userId, string property) {
			if (property == null)
				throw new ArgumentNullException(nameof(property));
			if (assemblingAccounts.TryGetValue(userId, out AccountAssemblingModel accountAssemblingModel)) {
				switch (accountAssemblingModel.AccountAssemblingStage) {
					case AccountAssemblingStage.AddAccountName:
						accountAssemblingModel.AccountName = property;
						break;
					case AccountAssemblingStage.AddLink:
						accountAssemblingModel.Link = property;
						break;
					case AccountAssemblingStage.AddNote:
						accountAssemblingModel.Note = property;
						break;
					case AccountAssemblingStage.AddLogin:
						accountAssemblingModel.Login = property;
						break;
					case AccountAssemblingStage.AddPassword:
						accountAssemblingModel.Password = property;
						break;
					case AccountAssemblingStage.EncryptPassword:
						accountAssemblingModel.Password = property;
						break;
					case AccountAssemblingStage.Release:
						throw new InvalidOperationException("Account is already assembled.");
					default:
						throw new InvalidOperationException("Unexpected Account Assembling Stage");
				}
				return ++accountAssemblingModel.AccountAssemblingStage;
			}
			throw new InvalidOperationException(
				"AccountAssembling doesn't exist. Use Create(int userId, string[] args) to start inline assembling");
		}

		public AccountAssemblingStage SkipStage(int userId, AccountAssemblingStageSkip accountAssemblingStageSkip) {
			if (!assemblingAccounts.TryGetValue(userId, out AccountAssemblingModel accountAssemblingModel)) {
				throw new InvalidOperationException("AccountAssembling doesn't exist.");
			}
			return accountAssemblingModel.AccountAssemblingStage = ((AccountAssemblingStage)accountAssemblingStageSkip) + 1;
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
