using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace UPwdBot.Commands {
	public interface IMessageCommand {
		Task ExecuteAsync(Message message, Types.User user);
	}
}
