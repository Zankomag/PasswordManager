
namespace PasswordManager.Bot.Commands.Enums {

	public enum UpdateAccountCommandCode {
		SelectUpdateType = '0',
		AccountName = 'A',
		Link = 'R',
		DeleteLink = 'r',
		Note = 'N',
		DeleteNote = 'n',
		Login = 'L',
		Password = 'P',
		OutdatedTime = 'T',
		UntrackOutdatedTime = 't',
		UseGlobalOutdatedTime = 'g',
		RemoveEncryption = 'D',
		AcceptPassword = 'U',
		SkipPasswordEncryption = 'S'
	}
}
