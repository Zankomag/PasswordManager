using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace UPwdBot.Commands {
	interface ILocalizedCommand : ICommand {
		Task ExecuteAsync(Message message, string langCode);
	}
}
