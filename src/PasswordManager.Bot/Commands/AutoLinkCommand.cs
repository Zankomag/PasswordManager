using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Core.Entities;
using User = PasswordManager.Core.Entities.User;
using PasswordManager.Bot.Commands.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class AutoLinkCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, User user) {
			if (PasswordManagerService.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
				if (account.AccountName != null) {
					account.Link = account.AccountName.AutoLink().BuildLink();
					PasswordManagerService.AssemblingAccounts[user.Id] = account;
					await AddAccountCommand.UpdateCallBackMessageAsync(
						callbackQuery.Message.Chat.Id,
						callbackQuery.Message.MessageId,
						account,
						user);
				} else {
					await BotHandlerService.Bot.AnswerCallbackQueryAsync(callbackQuery.Id,
					text: Localization.GetMessage("NoAccName", user.Lang), showAlert: true);
				}
			} else {
				await BotHandlerService.Bot.AnswerCallbackQueryAsync(callbackQuery.Id,
					text: Localization.GetMessage("CantWithoutNewAcc", user.Lang), showAlert: true);
			}
		}
	}
}
