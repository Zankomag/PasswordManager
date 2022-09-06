using PasswordManager.Telegram.Commands.Enums;

namespace PasswordManager.Telegram.Services.Enums; 

public enum AccountUpdatingStage {
	None = 0,
	Release = 1,
	AccountName = UpdateAccountCommandCode.AccountName,
	Link = UpdateAccountCommandCode.Link,
	Note = UpdateAccountCommandCode.Note,
	Login = UpdateAccountCommandCode.Login,
	Password = UpdateAccountCommandCode.Password,
	EncryptPassword = AccountAssemblingStage.AddEncryptionKey,
	RemovePasswordEncryption = '@',
	OutdatedTime = UpdateAccountCommandCode.OutdatedTime,
}