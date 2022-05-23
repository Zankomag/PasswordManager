
using PasswordManager.Common.Abstractions;

namespace PasswordManager.Bot.Settings;

/// <summary>
/// Telegram Bot Settings
/// </summary>
public class BotSettings : ISettings {
	
	/// <inheritdoc />
	public static string SectionName => "TelegramBot";
	
	public string Token { get; set; }
	public long[] AdminIds { get; set; }
	//TODO WARNING: this setting would be better set manually each time it's required, because on serverless setWebhook can be called per each request
	/// <summary>
	/// Domain which Telegram will send webhook requests to
	/// </summary>
	public string Domain { get; set; }
	/// <summary>
	/// Indicates whether bot is accessible to everyone 
	/// 
	/// <para>If bot is public, he will registered every new user. 
	/// If bot is private new users registration should be restricted.
	/// For example, new users can be registered manually by admins
	/// or new user must enter registration code he received somewhere else.</para>
	/// 
	/// <para>If bot is considered as private, it's up to you to decide how he will 
	/// handle requests from unregistered users. He can just ignore them pretending he is dead, 
	/// he can answer that he works only for registered users, he can send admin contact or whatever.</para>
	/// </summary>
	public bool IsPublic { get; set; }

}