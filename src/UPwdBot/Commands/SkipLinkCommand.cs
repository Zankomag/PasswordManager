using System.Threading.Tasks;
using Telegram.Bot.Types;
using UPwdBot.Types;
using Uten.Localization.MultiUser;

namespace UPwdBot.Commands {
	public class SkipLinkCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			if (PasswordManager.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
				account.SkipLink = true;
				if (account.Link != null)
					account.Link = null;
				PasswordManager.AssemblingAccounts[user.Id] = account;
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
