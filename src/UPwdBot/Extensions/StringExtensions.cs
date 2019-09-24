using UPwdBot.Types.Enums;
namespace UPwdBot.Extensions {
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

		public static string ToEmojiString(this bool param, bool addSpace = false) {
			string result =  param ? "✅" : "✖️";
			if (addSpace)
				result += " ";
			return result;
		}

		public static string ToStringCode(this CallbackCommandCode callbackCommandCode) {
			return ((char)callbackCommandCode).ToString();
		}

		public static string ToStringCode(this SetUpPasswordCommandCode setUpPasswordCommandCode) {
			return ((char)setUpPasswordCommandCode).ToString();
		}
	}
}
