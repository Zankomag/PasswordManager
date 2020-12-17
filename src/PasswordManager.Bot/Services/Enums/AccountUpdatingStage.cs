using PasswordManager.Bot.Commands.Enums;

namespace PasswordManager.Bot.Services.Enums {
	public enum AccountUpdatingStage {
		None = 0,
		Release = 1,
		AccountName = UpdateAccountCommandCode.AccountName,
		Link = UpdateAccountCommandCode.Link,
		Note = UpdateAccountCommandCode.Note,
		Login = UpdateAccountCommandCode.Login,
		Password = UpdateAccountCommandCode.Password,
		EncryptPassword = AccountAssemblingStage.AddEncryptionKey,
		SkipPasswordEncryption = '@',
		OutdatedTime = UpdateAccountCommandCode.OutdatedTime,
	}
}
