namespace PasswordManager.Bot.Enums {

	public enum CallbackCommandCode { 
		 SelectLanguage = 'L',
		 SkipLink = 'S',  
		 AutoLink = 'A',
		 GeneratePassword = 'G',
		 AcceptPassword = 'Z',
		 Search = 'Q', 
		 ShowPassword = 'P',
		 ShowAccount = 'O',
		 DeleteMessage = 'D',
		 UpdateAccount = 'U',
		 DeleteAccount = 'X',
		 SetUpPasswordGenerator = 'T',
		 EnterEncryptionyonKey = 'E',
		 ShowEncryptionKeyHint = 'H'
	}
}
