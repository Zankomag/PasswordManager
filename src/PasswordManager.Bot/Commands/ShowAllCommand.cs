using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using User = PasswordManager.Core.Entities.User;

namespace PasswordManager.Bot.Commands {
	public class ShowAllCommand : IMessageCommand {

		public async Task ExecuteAsync(Message message, User user) {
			await PasswordManagerHandler.SearchAccounts(message.From.Id, user.Lang);
		}

	}
}
