﻿
namespace PasswordManager.Bot.Types.Enums {
	public enum SetUpPasswordCommandCode {
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