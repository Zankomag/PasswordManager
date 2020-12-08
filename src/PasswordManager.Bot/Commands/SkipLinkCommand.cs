using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot;
using MultiUserLocalization;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Models;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class SkipLinkCommand : Abstractions.BotCommand, ICallbackQueryCommand {
		private readonly IAccountService accountService;

		public SkipLinkCommand(IBotService botService, IAccountService accountService) : base(botService) {
			this.accountService = accountService;
		}

		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			if (.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
				//TODO
				//ADD SKIPLINK CKECK
				//account.SkipLink = true;
				if (account.Link != null)
					account.Link = null;
				.AssemblingAccounts[user.Id] = account;
				await AddAccountCommand.UpdateCallBackMessageAsync(
					callbackQuery.Message.Chat.Id,
					callbackQuery.Message.MessageId,
					account,
					user);
			} else {
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id,
					text: Localization.GetMessage("CantWithoutNewAcc", user.Lang));
			}
		}
	}
}
