using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using UPwdBot.Types;

namespace UPwdBot.Commands {
	public class AutoLinkCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			if (BotHandler.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
				account.Link = account.AccountName.AutoLink().BuildLink();
					BotHandler.AssemblingAccounts[user.Id] = account;
				await AddAccountCommand.UpdateCallBackMessageAsync(
					callbackQuery.Message.Chat.Id,
					callbackQuery.Message.MessageId,
					account,
					user.Lang);
			} else {
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id,
					text: Localization.GetMessage("CantWithoutNewAcc", user.Lang), showAlert: true);
			}
		}
	}
}
