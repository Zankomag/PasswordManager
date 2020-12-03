using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MultiUserLocalization {
	public class Localization {

		private static Dictionary<string, Dictionary<string, string>> stringsByCode = new Dictionary<string, Dictionary<string, string>>();
		public static int LanguageNumber { get => stringsByCode.Count; }

		public static readonly string defaultLanguage = "en";
		private const string defaultIcon = "❔";
		/// <summary>
		/// </summary>
		/// <returns>String of <paramref name="langCode"/> by its key. Retunrs en-US string if not found.</returns>
		public static string GetMessage(string key, string langCode) {
			if (langCode != defaultLanguage && stringsByCode[langCode].ContainsKey(key))
				return stringsByCode[langCode][key];
			else 
				return stringsByCode[defaultLanguage][key];
		}
		static Localization() {
			if(!File.Exists(Path.Combine("Locales", defaultLanguage + ".json")))
				throw new FileNotFoundException("Default Language file couldn't be found in Locales folder.", defaultLanguage + ".json");
			string[] files = Directory.GetFiles("Locales");
			for(int i = 0; i < files.Length; i++) {
				Dictionary<string, string> strings;
				using (StreamReader file = File.OpenText(files[i])) {
					JsonSerializer serializer = new JsonSerializer();
					strings = (Dictionary<string, string>)serializer
						.Deserialize(file, typeof(Dictionary<string, string>));
				}
				stringsByCode.Add(Path.GetFileNameWithoutExtension(files[i]), strings);
			}	
		}

		public static IList<string> GetIcons() {
			IList<string> icons = new List<string>();
			foreach(var item in stringsByCode) {
				if (item.Value.ContainsKey("Icon")) {
					icons.Add(item.Value["Icon"]);
				}
				else
					icons.Add(defaultIcon);
			}
			return icons;
		}

		public static IList<string> GetLangCodes() {
			IList<string> langCodes = new List<string>();
			foreach (var item in stringsByCode) {
				langCodes.Add(item.Key);
			}
			return langCodes;
		}

		public static bool ContainsLanguage(string langCode) {
			if (langCode == null) return false;
			return stringsByCode.ContainsKey(langCode);
		}
	}
}
