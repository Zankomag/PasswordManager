using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Commands {
	public interface ICallBackQueryCommand {
		Task ExecuteAsync(CallbackQuery callbackQuery, global::PasswordManager.Types.User user);
	}
}
