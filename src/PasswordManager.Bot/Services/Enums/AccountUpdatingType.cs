using PasswordManager.Bot.Commands.Enums;

namespace PasswordManager.Bot.Services.Enums {
	public enum AccountUpdatingType {
		AccountName = UpdateAccountCommandCode.AccountName,
		Link = UpdateAccountCommandCode.Link,
		Note = UpdateAccountCommandCode.Note,
		Login = UpdateAccountCommandCode.Login,
		Password = UpdateAccountCommandCode.Password,
		OutdatedTime = UpdateAccountCommandCode.OutdatedTime
	}
}
