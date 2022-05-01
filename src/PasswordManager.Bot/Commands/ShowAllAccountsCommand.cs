using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class ShowAllAccountsCommand : Abstractions.BotCommand, IMessageCommand {
		private readonly IAccountService accountService;
		private readonly IBotUi botUi;

		public ShowAllAccountsCommand(IBot bot, IAccountService accountService, IBotUi botUi) : base(bot) {
			this.accountService = accountService;
			this.botUi = botUi;
		}

		async Task IMessageCommand.ExecuteAsync(Message message, BotUser botUser) {
			var result = await accountService.GetAccountsByNameAsync()
		}

	}
}
