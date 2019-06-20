
namespace UPwdBot {
	public static class LinkBuilder {

		public static string BuildLink(this string link) {
			return (link.StartsWith("https://") || link.StartsWith("http://"))
				? link.Trim() : "http://" + link.Trim();
		}

		/// <summary>Returns first_word_in_string.com</summary>
		public static string AutoLink(this string str) {
			string autoLink = "";
			autoLink += str.Contains(' ') ?
								str.Substring(0,
									str.IndexOf(' ')).ToLower() :
								str.ToLower();
			autoLink += ".com";
			return autoLink;
		}
	}
}
