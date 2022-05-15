using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;


namespace PasswordManager.Bot;

//todo move to nuget package
public class Localization {
	//TODO:
	//Add class with all text variables and
	//if variable has arguments that it has to be private and accept needed variables
	//Add extension method for string that returns right localization object by langCode

	private static readonly Dictionary<string, Dictionary<string, string>> stringsByCode = new Dictionary<string, Dictionary<string, string>>();
	public static int LanguageNumber => stringsByCode.Count;

	public const string DefaultLanguageCode = "en";
	private const string defaultIcon = "❔";
	private const string iconKey = "Icon";
	/// <summary>
	/// </summary>
	/// <returns>String of <paramref name="langCode"/> by its key. Retunrs en-US string if not found.</returns>
	public static string GetMessage(string key, string langCode) {
		if (langCode != DefaultLanguageCode && stringsByCode[langCode].ContainsKey(key))
			return stringsByCode[langCode][key];
		return stringsByCode[DefaultLanguageCode][key];
	}
	static Localization() {
		//todo re-implement this module in normal manner...
		if(!File.Exists(Path.Combine("Locales", DefaultLanguageCode + ".json")))
			throw new FileNotFoundException("Default Language file couldn't be found in Locales folder.", DefaultLanguageCode + ".json");
		string[] fileNames = Directory.GetFiles("Locales");
		foreach(string fileName in fileNames) {
			Dictionary<string, string> strings;
			using (StreamReader file = File.OpenText(fileName)) {
				JsonSerializer serializer = new JsonSerializer();
				strings = (Dictionary<string, string>)serializer
					.Deserialize(file, typeof(Dictionary<string, string>));
			}
			stringsByCode.Add(Path.GetFileNameWithoutExtension(fileName), strings);
		}
	}

	public static IList<string> GetIcons() {
		IList<string> icons = new List<string>();
		foreach(var item in stringsByCode) {
			if (item.Value.ContainsKey(iconKey)) {
				icons.Add(item.Value[iconKey]);
			}
			else
				icons.Add(defaultIcon);
		}
		return icons;
	}

	public static IList<string> GetLangCodes() => stringsByCode.Keys.ToList();

	public static bool ContainsLanguage(string langCode) {
		if (langCode == null) return false;
		return stringsByCode.ContainsKey(langCode);
	}
}