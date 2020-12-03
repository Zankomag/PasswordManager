using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Commands.Abstractions {
	public interface IMessageCommand : ICommand {
		Task ExecuteAsync(Message message, Core.Entities.User user);
	}
}
