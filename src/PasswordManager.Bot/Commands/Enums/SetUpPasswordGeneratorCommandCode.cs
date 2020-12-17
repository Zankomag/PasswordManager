
namespace PasswordManager.Bot.Commands.Enums {
	public enum SetUpPasswordGeneratorCommandCode {
		/// <summary>
		/// Keyboard contains Generate button
		///  </summary>
		None = '0',
		/// <summary>
		/// Keyboard contains Back button instead of Generate button
		/// and returns to generating password for account assembling
		/// </summary>
		ReturnAssembling = '1',
		/// <summary>
		/// Keyboard contains Back button instead of Generate button
		/// and returns to generating password for account updating
		/// </summary>
		ReturnUpdating = '2',
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
