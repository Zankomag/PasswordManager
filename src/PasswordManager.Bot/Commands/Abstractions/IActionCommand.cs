using PasswordManager.Bot.Models;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Commands.Abstractions {
	/// <summary>
	/// This command processes botUser messages that are not starting with '/' and does not contain reply
	/// </summary>
	public interface IActionCommand : IBotCommand {
		Task ExecuteAsync(Message message, BotUser botUser);
	}
}
