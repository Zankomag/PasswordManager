using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class ShowAllAccountsCommand : Abstractions.BotCommand, IMessageCommand {
		private readonly IAccountService accountService;

		public ShowAllAccountsCommand(IBot bot, IAccountService accountService) : base(bot) {
			this.accountService = accountService;
		}

		async Task IMessageCommand.ExecuteAsync(Message message, BotUser botUser) {
			await .SearchAccounts(message.From.Id, botUser.Lang);
		}

	}
}
