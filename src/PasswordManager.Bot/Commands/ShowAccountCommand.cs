using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class ShowAccountCommand : Abstractions.BotCommand, ICallbackQueryCommand {
		private readonly IAccountService accountService;

		public ShowAccountCommand(IBot botService, IAccountService accountService) : base(botService) {
			this.accountService = accountService;
		}

		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			await botService.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			string accountId = callbackQuery.Data.Substring(1);

			await .ShowAccountById(
				callbackQuery.From.Id,
				accountId,
				user.Lang,
				callbackQuery.Message.MessageId);
		}

	}
}
