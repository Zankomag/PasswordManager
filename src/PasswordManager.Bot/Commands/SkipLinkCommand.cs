using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Models;

namespace PasswordManager.Bot.Commands {
	public class SkipLinkCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			if (PasswordManagerService.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
				//TODO
				//ADD SKIPLINK CKECK
				//account.SkipLink = true;
				if (account.Link != null)
					account.Link = null;
				PasswordManagerService.AssemblingAccounts[user.Id] = account;
				await AddAccountCommand.UpdateCallBackMessageAsync(
					callbackQuery.Message.Chat.Id,
					callbackQuery.Message.MessageId,
					account,
					user);
			} else {
				await BotHandlerService.Bot.AnswerCallbackQueryAsync(callbackQuery.Id,
					text: Localization.GetMessage("CantWithoutNewAcc", user.Lang));
			}
		}
	}
}
