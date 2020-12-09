
namespace PasswordManager.Bot.Enums {
	public enum AccountAssemblingStage {
		None = -1,
		AddAccountName = 0,
		AddLink,
		AddNote,
		AddLogin,
		AddPassword,
		AddEncryptionKey,
		Release
	}
}
