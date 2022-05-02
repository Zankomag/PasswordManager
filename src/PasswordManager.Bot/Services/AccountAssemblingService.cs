using System;
using System.Collections.Generic;
using PasswordManager.Application.Encryption;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Bot.Models;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Services.Enums;
using PasswordManager.Bot.Extensions;

namespace PasswordManager.Bot.Services; 

public class AccountAssemblingService : IAccountAssemblingService {
	//Assembling Accounts data is stored in memory
	//because storing it in database doesn't worth it
	private readonly Dictionary<long, AccountAssemblingModel> accountAssemblings;

	public AccountAssemblingService() {
		accountAssemblings = new Dictionary<long, AccountAssemblingModel>();
	}

	public void Cancel(long userId) => accountAssemblings.Remove(userId); 

	// [0] /add => Ask for AccountName => Ask for Link => Ask for Note => Ask for password => Ask for EncryptionKey
	// [1] /add AccountName => Ask for Link => Ask for Note => Ask for password => Ask for EncryptionKey
	// [2] /add AccountName \n Login => Ask for password => Ask for EncryptionKey
	// [3] /add AccountName \n Login \n Password => Ask for EncryptionKey
	// [4] /add AccountName \n Login \n Password \n EncryptionKey
	// [5] /add AccountName \n Link \n Login \n Password \n EncryptionKey
	// [6] /add AccountName \n Link \n Note \n Login \n Password \n EncryptionKey
	public AccountAssemblingStage Create(long userId, string[] args = null) {
		var accountAssemblingModel = new AccountAssemblingModel() {
			AccountAssemblingStage = AccountAssemblingStage.AddAccountName,
			UserId = userId,
		};

		if (args == null || args.Length == 0) {
			accountAssemblings[userId] = accountAssemblingModel;
			return accountAssemblingModel.AccountAssemblingStage;
		}

		accountAssemblingModel.AccountName = args[0];
		switch (args.Length) {
			case 1:
				accountAssemblingModel.AccountAssemblingStage = AccountAssemblingStage.AddLink;
				break;
			case 2:
				accountAssemblingModel.Login = args[1];
				accountAssemblingModel.AccountAssemblingStage = AccountAssemblingStage.AddPassword;
				break;
			case 3:
				accountAssemblingModel.Login = args[1];
				accountAssemblingModel.Password = args[2];
				accountAssemblingModel.AccountAssemblingStage = AccountAssemblingStage.AddEncryptionKey;
				break;
			case 4:
				accountAssemblingModel.Login = args[1];
				accountAssemblingModel.Password = args[2].Encrypt(args[3]);
				accountAssemblingModel.Encrypted = true;
				accountAssemblingModel.AccountAssemblingStage = AccountAssemblingStage.Release;
				break;
			case 5:
				accountAssemblingModel.Link = args[1];
				accountAssemblingModel.Login = args[2];
				accountAssemblingModel.Password = args[3].Encrypt(args[4]);
				accountAssemblingModel.Encrypted = true;
				accountAssemblingModel.AccountAssemblingStage = AccountAssemblingStage.Release;
				break;
			default:
				accountAssemblingModel.Link = args[1];
				accountAssemblingModel.Note = args[2];
				accountAssemblingModel.Login = args[3];
				accountAssemblingModel.Password = args[4].Encrypt(args[5]);
				accountAssemblingModel.Encrypted = true;
				accountAssemblingModel.AccountAssemblingStage = AccountAssemblingStage.Release;
				break;
		}

		accountAssemblings[userId] = accountAssemblingModel;
		return accountAssemblingModel.AccountAssemblingStage;
	}

	//todo check if validation works, also we need to validate properties one by one on the assembling stage
	public Account Release(long userId) {
		if (this.accountAssemblings.TryGetValue(userId, out AccountAssemblingModel accountAssemblingModel) 
			&& accountAssemblingModel.AccountAssemblingStage == AccountAssemblingStage.Release) {

			this.accountAssemblings.Remove(userId);
			return new Account {
				AccountName = accountAssemblingModel.AccountName,
				UserId = accountAssemblingModel.UserId,
				Link = accountAssemblingModel.Link,
				Note = accountAssemblingModel.Note,
				Login = accountAssemblingModel.Login,
				Password = accountAssemblingModel.Password,
				Encrypted = accountAssemblingModel.Encrypted,
				OutdatedTime = new TimeSpan(0, 0, 0),
				PasswordUpdatedDate = DateTime.UtcNow.Date,
			};
		}
		return null;
	}

	public AccountAssemblingStage Assemble(long userId, string property,
		AccountAssemblingStage expectedAccountAssemblingStage = AccountAssemblingStage.None) {
		if (property == null)
			throw new ArgumentNullException(nameof(property));
		if (accountAssemblings.TryGetValue(userId, out AccountAssemblingModel accountAssemblingModel)) {
			if (expectedAccountAssemblingStage != AccountAssemblingStage.None
				&& accountAssemblingModel.AccountAssemblingStage != expectedAccountAssemblingStage) {
				throw new InvalidOperationException("Actual AssemblingStage doesn't match the expected");
			}
			switch (accountAssemblingModel.AccountAssemblingStage) {
				case AccountAssemblingStage.AddAccountName:
					accountAssemblingModel.AccountName = property;
					break;
				case AccountAssemblingStage.AddLink:
					accountAssemblingModel.Link = property.AutoDomain().BuildUrl();
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
				case AccountAssemblingStage.AddEncryptionKey:
					accountAssemblingModel.Password = accountAssemblingModel.Password.Encrypt(property);
					accountAssemblingModel.Encrypted = true;
					break;
				case AccountAssemblingStage.Release:
					return accountAssemblingModel.AccountAssemblingStage;
				default:
					throw new InvalidOperationException("Unexpected Account Assembling Stage");
			}
			return ++accountAssemblingModel.AccountAssemblingStage;
		}
		throw new InvalidOperationException(
			"AccountAssembling doesn't exist. Use Create(int userId, string[] args) to start inline assembling");
	}

	public AccountAssemblingStage SkipStage(long userId, AccountAssemblingStageSkip accountAssemblingStageSkip) {
		if (!accountAssemblings.TryGetValue(userId, out AccountAssemblingModel accountAssemblingModel))
			throw new InvalidOperationException("AccountAssembling doesn't exist.");
		if(accountAssemblingModel.AccountAssemblingStage == (AccountAssemblingStage)accountAssemblingStageSkip)
			return ++accountAssemblingModel.AccountAssemblingStage;
		throw new InvalidOperationException("AccountAssembling is on other stage.");
	}

	public string GetAccountName(long userId) {
		if (accountAssemblings.TryGetValue(userId, out AccountAssemblingModel accountAssemblingModel)) {
			if (accountAssemblingModel.AccountName != null)
				return accountAssemblingModel.AccountName;
		}
		throw new InvalidOperationException("AssemblingAccount doesn't have name yet");
	}
}