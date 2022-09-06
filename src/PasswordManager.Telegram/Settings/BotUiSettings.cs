using Junetic.Common.Abstractions;

namespace PasswordManager.Telegram.Settings; 

public class BotUiSettings : ISettings {

	/// <inheritdoc />
	public static string SectionName => "TelegramBotUi";

	/// <summary>
	/// Indicates how many accounts can be on a page
	/// </summary>
	public int PageSize { get; set; }
	
	/// <summary>
	/// Indicates separating between account records on a page
	/// </summary>
	public string AccountSeparator { get; set; }

	
}