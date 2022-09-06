using System.Text;
using System;
using System.Linq;
using PasswordManager.Telegram.Commands.Enums;

namespace PasswordManager.Telegram.Extensions; 

public static class StringExtensions {
	//todo validate all Url types, not only http, and improve Url building logic
	///<summary></summary>
	///<returns>https://value or http://value</returns>
	public static string BuildUrl(this string value) {
		if(String.IsNullOrWhiteSpace(value))
			throw new ArgumentException("string is null, whitespace or empty");
		return value.StartsWith("https://", StringComparison.Ordinal)
			|| value.StartsWith("http://", StringComparison.Ordinal)
				? value.Trim()
				: "https://" + value.Trim();
	}

	///<summary></summary>
	/// ///<returns>first_word_in_string.com</returns>
	public static string AutoDomain(this string value) {
		if (String.IsNullOrWhiteSpace(value))
			throw new ArgumentException("string is null, whitespace or empty");
		int spaceIndex;
		string autoLink = (spaceIndex = value.IndexOf(' ')) == -1
			? value.ToLower() : value[..spaceIndex].ToLower();
		StringBuilder autoLinkBuilder = new StringBuilder(autoLink);
		int dotIndex;
		if ((dotIndex = autoLink.IndexOf('.')) == -1) {
			if(dotIndex == autoLink.Length-1)
				autoLinkBuilder.Append("com");
		} else {
			autoLinkBuilder.Append(".com");
		}
		return autoLinkBuilder.ToString();
	}

	/// <summary>
	/// Converts bool to "1" or "0"
	/// </summary>
	/// <param name="param"></param>
	/// <returns>"1" or "0"</returns>
	public static string ToZeroOneString(this bool param) => param ? "1" : "0";

	/// <summary>
	/// Converts True to "0" and False to "1", because it will be handled as new setting
	/// which must be opposite to last setting
	/// </summary>
	/// <param name="param"></param>
	/// <returns></returns>
	public static string ToReverseZeroOneString(this bool param) => param ? "0" : "1";

	public static string ToEmojiString(this bool param, bool addSpaceInEnd = false) {
		string result =  param ? "✅" : "✖️";
		if (addSpaceInEnd)
			result += " ";
		return result;
	}

	public static string ToStringCode(this CallbackQueryCommandCode callbackCommandCode) 
		=> ((char)callbackCommandCode).ToString();

	///<summary></summary>
	/// <param name="setUpPasswordGeneratorCommandCode"></param>
	/// <returns><see cref="CallbackQueryCommandCode.SetUpPasswordGenerator"/> + setUpPasswordGeneratorCommandCode</returns>
	public static string ToStringCode(this SetUpPasswordGeneratorCommandCode setUpPasswordGeneratorCommandCode)
		=> GetStringCode(CallbackQueryCommandCode.SetUpPasswordGenerator, (char)setUpPasswordGeneratorCommandCode);

	///<summary></summary>
	/// <param name="addAccountCommandCode"></param>
	/// <returns><see cref="CallbackQueryCommandCode.AddAccount"/> + addAccountCommandCode</returns>
	public static string ToStringCode(this AddAccountCommandCode addAccountCommandCode)
		=> GetStringCode(CallbackQueryCommandCode.AddAccount, (char)addAccountCommandCode);

	///<summary></summary>
	/// <param name="selectLanguageCommandCode"></param>
	/// <returns><see cref="CallbackQueryCommandCode.SelectLanguage"/> + selectLanguageCommandCode</returns>
	public static string ToStringCode(this SelectLanguageCommandCode selectLanguageCommandCode) 
		=> GetStringCode(CallbackQueryCommandCode.SelectLanguage, (char)selectLanguageCommandCode);

	///<summary></summary>
	/// <param name="updateAccountCommandCode"></param>
	/// <param name="accountId">Id of account to update</param>
	/// <returns><see cref="CallbackQueryCommandCode.UpdateAccount"/> + updateAccountCommandCode + accountId</returns>
	public static string ToStringCode(this UpdateAccountCommandCode updateAccountCommandCode, long accountId)
		=> GetStringCode(CallbackQueryCommandCode.UpdateAccount, (char)updateAccountCommandCode, accountId);

	///<summary></summary>
	/// <param name="updateAccountCommandCode"></param>
	/// <param name="accountId">Id of account to update</param>
	/// <returns><see cref="CallbackQueryCommandCode.UpdateAccount"/> + updateAccountCommandCode + accountId</returns>
	public static string ToStringCode(this UpdateAccountCommandCode updateAccountCommandCode, string accountId)
		=> GetStringCode(CallbackQueryCommandCode.UpdateAccount, (char)updateAccountCommandCode, accountId);

	///<summary></summary>
	/// <param name="updateAccountCommandCode"></param>
	/// <returns><see cref="CallbackQueryCommandCode.UpdateAccount"/> + updateAccountCommandCode</returns>
	public static string ToStringCode(this UpdateAccountCommandCode updateAccountCommandCode)
		=> GetStringCode(CallbackQueryCommandCode.UpdateAccount, (char)updateAccountCommandCode);

	///<summary></summary>
	/// <param name="deleteAccountCommandCode"></param>
	/// /// <param name="accountId">Id of account to update</param>
	/// <returns><see cref="CallbackQueryCommandCode.DeleteAccount"/> + deleteAccountCommandCode</returns>
	public static string ToStringCode(this DeleteAccountCommandCode deleteAccountCommandCode, long accountId)
		=> GetStringCode(CallbackQueryCommandCode.DeleteAccount, (char)deleteAccountCommandCode, accountId);

	///<summary></summary>
	/// <param name="generatePasswordCommandCode"></param>
	/// <returns><see cref="CallbackQueryCommandCode.GeneratePassword"/> + generatePasswordCommandCode</returns>
	public static string ToStringCode(this GeneratePasswordCommandCode generatePasswordCommandCode)
		=> GetStringCode(CallbackQueryCommandCode.GeneratePassword, (char)generatePasswordCommandCode);

	/// <summary></summary>
	/// <param name="callbackQueryCommandCode"></param>
	/// <param name="additionalCommand">Command that need to be appended to <paramref name="callbackQueryCommandCode"/></param>
	/// <returns><paramref name="callbackQueryCommandCode"/> + <paramref name="additionalCommand"/></returns>
	private static string GetStringCode(CallbackQueryCommandCode callbackQueryCommandCode, char additionalCommand)
		=> new StringBuilder(callbackQueryCommandCode.ToStringCode()).Append(additionalCommand).ToString();

	/// <summary></summary>
	/// <param name="callbackQueryCommandCode"></param>
	/// <param name="additionalCommand">Command that need to be appended to <paramref name="callbackQueryCommandCode"/></param>
	/// <param name="param"></param>
	/// <returns><paramref name="callbackQueryCommandCode"/> + <paramref name="additionalCommand"/></returns>
	private static string GetStringCode(CallbackQueryCommandCode callbackQueryCommandCode, char additionalCommand, object param)
		=> new StringBuilder(callbackQueryCommandCode.ToStringCode())
			.Append(additionalCommand)
			.Append(param)
			.ToString();

	///returns null if there is no command in message
	public static string GetTextCommand(this string messageText) {  
		//TODO:
		//remove '/' from returned message when using new commands without '/' 
		//that means - when support for commands without '/' will be implemented here
		if (messageText == null)
			throw new ArgumentNullException(nameof(messageText));
		//Command that starts with '/' may contain args and must be separated from them
		if (messageText.StartsWith('/')) {
			string commandString = messageText.ToLower();
			int cIndex = commandString.IndexOfAny(new char[] { ' ', '\n' });
			return cIndex != -1 ? commandString[..cIndex] : commandString;
		}
		return null;
	}

		
		
	//TODO:
	//unit test
	/// <param name="commandText">command with args</param>
	/// <returns>All commands args separated by new line, except command itself</returns>
	public static string[] GetCommandArgsByNewLine(this string commandText) {
		if (commandText == null)
			throw new ArgumentNullException(nameof(commandText));
		if (commandText[0] != '/')
			throw new ArgumentException("command must start with '/'", nameof(commandText));
		int indexOfSpace = commandText.IndexOf(' ');
		int indexOfNewLine = commandText.IndexOf('\n');
		int firstArgStartIndex; //Equals to length of /command + space|\n after it
		//Assign to firstArgStartIndex lowest index + 1 if it's not -1
		if (indexOfSpace == -1) {
			if (indexOfNewLine == -1)
				return null;
			firstArgStartIndex = indexOfNewLine + 1;
		} else {
			firstArgStartIndex = indexOfNewLine == -1 
				? indexOfSpace + 1
				: indexOfNewLine < indexOfSpace
					? indexOfNewLine + 1 : indexOfSpace + 1;
		}
		return commandText.Remove(0, firstArgStartIndex)
			.Split('\n', StringSplitOptions.RemoveEmptyEntries);
	}

	//Chars that need to be escaped in Telegram MarkdownV2
	private static readonly string[] charsToEscape 
		= new[] { '\\', '`', '_', '*', '[', ']', '(', ')', '~', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' }
			.Select(x => x.ToString()).ToArray();
	private static readonly string[] escapedChars = charsToEscape.Select(x => String.Concat('\\', x)).ToArray();

	public static string EscapeMarkdownV2Chars(this string value) {
		for(int i = 0; i< charsToEscape.Length; i++) {
			value = value.Replace(charsToEscape[i], escapedChars[i]);
		}
		return value;
	}

	public static string EscapeCodeBlockMarkdownV2Chars(this string value)
		=> value.Replace(charsToEscape[0], escapedChars[0])
			.Replace(charsToEscape[1], escapedChars[1]);

	public static string EscapeInlineLinkMarkdownV2Chars(this string value)
		=> value.Replace(charsToEscape[0], escapedChars[0])
			.Replace(charsToEscape[7], escapedChars[7]);


}