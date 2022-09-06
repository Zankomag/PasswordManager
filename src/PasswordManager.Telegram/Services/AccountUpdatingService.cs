using PasswordManager.Application.Encryption;
using PasswordManager.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PasswordManager.Telegram.Extensions;
using PasswordManager.Telegram.Models;
using PasswordManager.Telegram.Services.Abstractions;
using PasswordManager.Telegram.Services.Enums;

namespace PasswordManager.Telegram.Services; 

public class AccountUpdatingService : IAccountUpdatingService {
	//Updating Accounts data is stored in memory
	//because storing it in database doesn't worth it
	private readonly Dictionary<long, AccountUpdatingModel> updatingAccounts;

	public AccountUpdatingService() {
		updatingAccounts = new Dictionary<long, AccountUpdatingModel>();
	}

	public void StartUpdatingRequest(long userId, Account account, AccountUpdatingStage accountUpdatingType)
		=> updatingAccounts[userId] = new AccountUpdatingModel(accountUpdatingType, account);


	public void FinishUpdatingRequest(long userId) => updatingAccounts.Remove(userId);

	public (long? accountId, AccountUpdatingStage) GetNextUpdatingStageAndAccountId(long userId, string property,
		AccountUpdatingStage expectedAccountUpdatingStage = AccountUpdatingStage.None) {

		AccountUpdatingStage nextUpdatingStage = GetNextUpdatingStage(userId, property,
			expectedAccountUpdatingStage);
		long? accountId = null;
		if (nextUpdatingStage != AccountUpdatingStage.None)
			accountId = updatingAccounts[userId].Account.Id;
		return (accountId, nextUpdatingStage);
	}

	public AccountUpdatingStage GetNextUpdatingStage(long userId, string property,
		AccountUpdatingStage expectedAccountUpdatingStage = AccountUpdatingStage.None, long? accountId = null) {
		if (updatingAccounts.TryGetValue(userId, out AccountUpdatingModel accountUpdatingModel)) {
			if ((accountId == null) || (accountId.Value == accountUpdatingModel.Account.Id
					&& expectedAccountUpdatingStage != AccountUpdatingStage.None
					&& accountUpdatingModel.AccountUpdatingStage == expectedAccountUpdatingStage)) {
				if (accountUpdatingModel.AccountUpdatingStage != AccountUpdatingStage.Release) {
					AccountAssemblingModel accountAssemblingModel = new AccountAssemblingModel();
					switch (accountUpdatingModel.AccountUpdatingStage) {
						case AccountUpdatingStage.OutdatedTime:
							if (property == null) {
								accountUpdatingModel.Account.OutdatedTime = null;
								return (accountUpdatingModel.AccountUpdatingStage = AccountUpdatingStage.Release);
							} else if (Int32.TryParse(property, out int outdatedTimeDays)) {
								accountUpdatingModel.Account.OutdatedTime
									= new TimeSpan(outdatedTimeDays, 0, 0, 0);
								return (accountUpdatingModel.AccountUpdatingStage = AccountUpdatingStage.Release);
							} else {
								throw new ValidationException("Invalid number");
							}
						case AccountUpdatingStage.AccountName:
							accountAssemblingModel.AccountName = property;
							accountUpdatingModel.Account.AccountName = accountAssemblingModel.AccountName;
							return (accountUpdatingModel.AccountUpdatingStage = AccountUpdatingStage.Release);
						case AccountUpdatingStage.Link:
							accountAssemblingModel.Link = property.AutoDomain().BuildUrl();
							accountUpdatingModel.Account.Url = accountAssemblingModel.Link;
							return (accountUpdatingModel.AccountUpdatingStage = AccountUpdatingStage.Release);
						case AccountUpdatingStage.Note:
							accountAssemblingModel.Note = property;
							accountUpdatingModel.Account.Note = accountAssemblingModel.Note;
							return (accountUpdatingModel.AccountUpdatingStage = AccountUpdatingStage.Release);
						case AccountUpdatingStage.Login:
							accountAssemblingModel.Login = property;
							accountUpdatingModel.Account.Login = accountAssemblingModel.Login;
							return (accountUpdatingModel.AccountUpdatingStage = AccountUpdatingStage.Release);
						case AccountUpdatingStage.Password:
							accountAssemblingModel.Password = property;
							accountUpdatingModel.Account.Password = accountAssemblingModel.Password;
							accountUpdatingModel.Account.PasswordUpdatedDate = DateTime.UtcNow;
							return (accountUpdatingModel.AccountUpdatingStage = AccountUpdatingStage.EncryptPassword);
						case AccountUpdatingStage.EncryptPassword:
							accountAssemblingModel.Password = accountUpdatingModel.Account.Password.Encrypt(property);
							accountUpdatingModel.Account.Password = accountAssemblingModel.Password;
							accountUpdatingModel.Account.Encrypted = true;
							return (accountUpdatingModel.AccountUpdatingStage = AccountUpdatingStage.Release);
						case AccountUpdatingStage.RemovePasswordEncryption:
							accountAssemblingModel.Password = property;
							accountUpdatingModel.Account.Password = accountAssemblingModel.Password;
							accountUpdatingModel.Account.PasswordUpdatedDate = DateTime.UtcNow;
							accountUpdatingModel.Account.Encrypted = false;
							return (accountUpdatingModel.AccountUpdatingStage = AccountUpdatingStage.Release);
						default:
							throw new InvalidOperationException("Unknown updating stage");
					}
				} else {
					return AccountUpdatingStage.Release;
				}
			}
		}
		return AccountUpdatingStage.None;
	}

	public AccountUpdatingStage SkipNextUpdatingStage(long userId, long accountId,
		AccountUpdatingStage accountUpdatingStageToSkip) {
		if (updatingAccounts.TryGetValue(userId, out AccountUpdatingModel accountUpdatingModel)) {
			if (accountId == accountUpdatingModel.Account.Id
				&& accountUpdatingModel.AccountUpdatingStage == accountUpdatingStageToSkip) {
				return accountUpdatingStageToSkip switch {
					AccountUpdatingStage.EncryptPassword
						=> (accountUpdatingModel.AccountUpdatingStage = AccountUpdatingStage.Release),
					_ => throw new ArgumentException("Unexpected updating stage to skip")
				};
			}
		}
		return AccountUpdatingStage.None;
	}

	public Account ReleaseAccount(long userId) {
		if (updatingAccounts.TryGetValue(userId, out AccountUpdatingModel accountUpdatingModel)) {
			if (accountUpdatingModel.AccountUpdatingStage == AccountUpdatingStage.Release) {
				FinishUpdatingRequest(userId);
				return accountUpdatingModel.Account;
			}
		}
		return null;
	}

}