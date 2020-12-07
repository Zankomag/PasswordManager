using PasswordManager.Bot.Models;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Commands.Abstractions {
	public interface ICallBackQueryCommand : ICommand {
		Task ExecuteAsync(CallbackQuery callbackQuery, BotUser user);
	}
}
