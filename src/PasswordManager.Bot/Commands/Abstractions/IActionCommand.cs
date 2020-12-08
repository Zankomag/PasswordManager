using PasswordManager.Bot.Models;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Commands.Abstractions {
	public interface IActionCommand : IBotCommand {
		Task ExecuteAsync(Message message, BotUser user);
	}
}
