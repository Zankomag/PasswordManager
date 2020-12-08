using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot;
using MultiUserLocalization;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Application.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class AutoLinkCommand : Abstractions.BotCommand, ICallbackQueryCommand {
		private readonly IAccountService accountService;

		public AutoLinkCommand(IBotService botService, IAccountService accountService) : base(botService) {
			this.accountService = accountService;
		}

		public async Task ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			if (.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
				if (account.AccountName != null) {
					account.Link = account.AccountName.AutoLink().BuildLink();
					.AssemblingAccounts[user.Id] = account;
					await AddAccountCommand.UpdateCallBackMessageAsync(
						callbackQuery.Message.Chat.Id,
						callbackQuery.Message.MessageId,
						account,
						user);
				} else {
					await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id,
					text: Localization.GetMessage("NoAccName", user.Lang), showAlert: true);
				}
			} else {
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id,
					text: Localization.GetMessage("CantWithoutNewAcc", user.Lang), showAlert: true);
			}
		}
	}
}
