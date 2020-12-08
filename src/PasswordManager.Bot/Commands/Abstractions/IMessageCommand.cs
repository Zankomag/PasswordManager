using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;

namespace PasswordManager.Bot.Commands.Abstractions {
	public interface IMessageCommand : IBotCommand {
		Task ExecuteAsync(Message message, BotUser user);
	}
}
