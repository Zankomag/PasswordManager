using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class ShowAccountCommand : Abstractions.BotCommand, ICallbackQueryCommand {
		private readonly IAccountService accountService;
		private readonly IBotUi botUi;

		public ShowAccountCommand(IBot bot, IAccountService accountService, IBotUi botUi) : base(bot) {
			this.accountService = accountService;
			this.botUi = botUi;
		}

		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			await Bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			string accountId = callbackQuery.Data[1..];

			await .ShowAccountById(
				callbackQuery.From.Id,
				accountId,
				user.Lang,
				callbackQuery.Message.MessageId);
		}

	}
}
