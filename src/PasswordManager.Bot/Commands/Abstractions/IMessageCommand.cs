using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;

namespace PasswordManager.Bot.Commands.Abstractions {
	public interface IMessageCommand : ICommand {
		Task ExecuteAsync(Message message, BotUser user);
	}
}
