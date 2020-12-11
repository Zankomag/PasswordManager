
namespace PasswordManager.Bot.Commands.Enums {
	public enum SetUpPasswordGeneratorCommandCode {
		ContainsLowerChars = 'L',
		ContainsUpperChars = 'U',
		ContainsDigits = 'D',
		ContainsSpecialChars = 'S',
		FirstCharIsLetter = 'F',
		ContainsSpace = 'A',
		Length = 'M',
		Generate = 'G'
	}
}
