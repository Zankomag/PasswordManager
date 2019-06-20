using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace UPwdBot.Commands {
	public interface ICallBackQueryCommand {
		Task ExecuteAsync(CallbackQuery callbackQuery, UPwdBot.Types.User user);
	}
}
