using PasswordManager.Common.Abstractions;

namespace PasswordManager.Bot.Settings; 

public class BotUiSettings : ISettings {

	
	public string SectionName => "TelegramBotUi";

	public int PageSize { get; set; }

}