using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace UPwdBot.Commands {
	public class ShowAllCommand : IMessageCommand {

		public async Task ExecuteAsync(Message message, Types.User user) {
			await PasswordManager.SearchAccounts(message.From.Id, user.Lang);
		}

	}
}
