using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;

namespace PasswordManager.Bot.Commands {
	public class ShowAllCommand : IMessageCommand {

		public async Task ExecuteAsync(Message message, BotUser user) {
			await PasswordManagerService.SearchAccounts(message.From.Id, user.Lang);
		}

	}
}
