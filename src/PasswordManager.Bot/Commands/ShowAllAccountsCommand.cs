using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class ShowAllAccountsCommand : Abstractions.BotCommand, IMessageCommand {
		private readonly IAccountService accountService;

		public ShowAllAccountsCommand(IBotService botService, IAccountService accountService) : base(botService) {
			this.accountService = accountService;
		}
		public async Task ExecuteAsync(Message message, BotUser user) {
			await .SearchAccounts(message.From.Id, user.Lang);
		}

	}
}
