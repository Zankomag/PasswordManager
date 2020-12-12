using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class ShowAllAccountsCommand : Abstractions.BotCommand, IMessageCommand {
		private readonly IAccountService accountService;

		public ShowAllAccountsCommand(IBot botService, IAccountService accountService) : base(botService) {
			this.accountService = accountService;
		}

		async Task IMessageCommand .ExecuteAsync(Message message, BotUser user) {
			await .SearchAccounts(message.From.Id, user.Lang);
		}

	}
}
