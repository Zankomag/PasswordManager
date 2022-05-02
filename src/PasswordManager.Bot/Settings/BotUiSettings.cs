using PasswordManager.Common.Abstractions;

namespace PasswordManager.Bot.Settings; 

public class BotUiSettings : ISettings {

	
	public string SectionName => "TelegramBotUi";

	/// <summary>
	/// Indicates how many accounts can be on a page
	/// </summary>
	public int PageSize { get; set; }
	
	/// <summary>
	/// Indicates separating between account records on a page
	/// </summary>
	public string AccountSeparator { get; set; }

}