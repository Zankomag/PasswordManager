using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace UPwdBot.Commands {
	public interface IBaseCommand : ICommand {
		Task ExecuteAsync(Message message);
	}
}
