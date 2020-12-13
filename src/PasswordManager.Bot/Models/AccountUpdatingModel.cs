using PasswordManager.Bot.Services.Enums;

namespace PasswordManager.Bot.Models {
	public class AccountUpdatingModel {
		public AccountUpdatingType AccountUpdatingType;
		public long AccountId;

		public AccountUpdatingModel(AccountUpdatingType accountUpdatingType, long accountId)
			=> (AccountUpdatingType, AccountId) = (accountUpdatingType, accountId);
	}
}
