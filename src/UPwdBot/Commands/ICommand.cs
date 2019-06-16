using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace UPwdBot.Commands {
	public interface ICommand {
		Task ExecuteAsync(Message message);
	}
}
