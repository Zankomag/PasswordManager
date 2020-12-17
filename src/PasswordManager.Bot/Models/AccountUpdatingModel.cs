using PasswordManager.Bot.Services.Enums;
using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Models {
	public class AccountUpdatingModel {
		public AccountUpdatingStage AccountUpdatingStage;
		public Account Account;

		public AccountUpdatingModel(AccountUpdatingStage accountUpdatingStage, Account account) {
			AccountUpdatingStage = accountUpdatingStage;
			Account = account;
		}
	}
}
