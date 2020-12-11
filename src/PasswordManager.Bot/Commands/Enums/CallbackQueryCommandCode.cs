namespace PasswordManager.Bot.Commands.Enums {

	public enum CallbackQueryCommandCode { 
		 SelectLanguage = 'L',
		 GeneratePassword = 'G',
		 Search = 'Q', 
		 ShowPassword = 'P',
		 ShowAccount = 'O',
		 DeleteMessage = 'D',
		 UpdateAccount = 'U',
		 DeleteAccount = 'X',
		 SetUpPasswordGenerator = 'T',
		 ShowEncryptionKeyHint = 'H'
	}
}
