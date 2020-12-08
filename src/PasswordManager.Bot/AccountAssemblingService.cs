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
			if(assemblingAccounts.TryGetValue(userId, out AccountAssemblingModel accountAssembleModel)) {
				return accountAssembleModel.AccountAssemblingStage;
			}
			return AccountAssemblingStage.None;
		}

		//TODO:
		//Delete this method if it's not used
		public AccountAssemblingStage GetNextStage(int userId) {
			if (assemblingAccounts.TryGetValue(userId, out AccountAssemblingModel accountAssembleModel)) {
				if ((int)accountAssembleModel.AccountAssemblingStage > (int)AccountAssemblingStage.Release)
					throw new InvalidOperationException("AccountAssembling is on last (Release) stage");
				return accountAssembleModel.AccountAssemblingStage + 1;
			}
			return AccountAssemblingStage.None;
		}

		public Account Release(int userId) {
			if (assemblingAccounts.TryGetValue(userId, out AccountAssemblingModel accountAssembleModel)) {
				if(accountAssembleModel.AccountAssemblingStage == AccountAssemblingStage.Release) {
					assemblingAccounts.Remove(userId);
					return new Account {
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
		//TODO: Add SkipLink method => Change stage++
		//TODO: Add SkipNote method => Change stage++
		//TODO: Add SetPasswordEncrypted method
		public AccountAssemblingStage Assemble(int userId, string property) {
			if (property == null)
				throw new ArgumentNullException(nameof(property));
			if (assemblingAccounts.TryGetValue(userId, out AccountAssemblingModel accountAssembleModel)) {
				switch (accountAssembleModel.AccountAssemblingStage) {
					case AccountAssemblingStage.AddAccountName:
						accountAssembleModel.AccountName = property;
						break;
					case AccountAssemblingStage.AddLink:
						accountAssembleModel.Link = property;
						break;
					case AccountAssemblingStage.AddNote:
						accountAssembleModel.Note = property;
						break;
					case AccountAssemblingStage.AddLogin:
						accountAssembleModel.Login = property;
						break;
					case AccountAssemblingStage.AddPassword:
						accountAssembleModel.Password = property;
						break;
				}
				return accountAssembleModel.AccountAssemblingStage++;
			}
			throw new InvalidOperationException(
				"AccountAssembling doesn't exist. Use Create(int userId, string[] args) to start inline assembling");
		}

		public AccountAssemblingStage SkipStage(int userId, AccountAssemblingStage accountAssemblingStage) {
			AccountAssemblingStage nextAccountAssemblingStage = accountAssemblingStage switch {
				AccountAssemblingStage.AddLink => AccountAssemblingStage.AddLink + 1,
				AccountAssemblingStage.AddNote => AccountAssemblingStage.AddNote + 1,
				AccountAssemblingStage.EncryptPassword => AccountAssemblingStage.EncryptPassword + 1,
				_ => throw new InvalidOperationException("Only allowed AssemblingStages are allowed to be skipped")
			};
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
