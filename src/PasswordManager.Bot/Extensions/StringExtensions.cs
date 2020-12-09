using PasswordManager.Bot.Enums;
using System;

namespace PasswordManager.Bot.Extensions {
	public static class StringExtensions {

		public static string BuildLink(this string value) {
			return (value.StartsWith("https://") || value.StartsWith("http://"))
				? value.Trim() : "https://" + value.Trim();
		}

		/// <summary>Returns first_word_in_string.com</summary>
		public static string AutoLink(this string value) {
			string autoLink = value.Contains(' ') ?
								value.Substring(0,
									value.IndexOf(' ')).ToLower() :
								value.ToLower();
			autoLink += ".com";
			return autoLink;
		}

		public static string ToZeroOneString(this bool param) {
			return param ? "1" : "0";
		}

		/// <summary>
		/// This function retunrs reverse bool because it will be handled as new setting
		/// which must be opposite to last setting
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		public static string ToReverseZeroOneString(this bool param) {
			return param ? "0" : "1";
		}

			public static string ToEmojiString(this bool param, bool addSpace = false) {
			string result =  param ? "✅" : "✖️";
			if (addSpace)
				result += " ";
			return result;
		}

		public static string ToStringCode(this CallbackQueryCommandCode callbackCommandCode) {
			return ((char)callbackCommandCode).ToString();
		}

		public static string ToStringCode(this SetUpPasswordCommandCode setUpPasswordCommandCode) {
			return ((char)setUpPasswordCommandCode).ToString();
		}

		/// <returns>null if there is no command in message</returns>
		public static string GetTextCommand(this string messageText) {
			//Command that starts with '/' may contain args and must be separated from them
			if (messageText.StartsWith('/')) {
				string commandString = messageText.ToLower();
				int cIndex = commandString.IndexOfAny(new char[] { ' ', '\n' });
				return cIndex != -1 ? commandString.Substring(0, cIndex) : commandString;
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
					? indexOfNewLine : indexOfSpace;
			}
			return commandText.Remove(0, firstArgStartIndex)
				.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
