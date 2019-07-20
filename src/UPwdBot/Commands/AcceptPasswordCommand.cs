using System.Threading.Tasks;
using Telegram.Bot.Types;
using UPwdBot.Types;
using Uten.Encryption;
using Uten.Localization.MultiUser;

namespace UPwdBot.Commands {
	public class AcceptPasswordCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			if (PasswordManager.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
				account.Password = callbackQuery.Message.Text.Encrypt();
				if (callbackQuery.Data.Length == 1) {
					PasswordManager.AssemblingAccounts[user.Id] = account;
				} else {
					//Update password is existing account
				}
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id);
				await AddAccountCommand.UpdateCallBackMessageAsync(
					callbackQuery.Message.Chat.Id,
					callbackQuery.Message.MessageId,
					account,
					user);
			}
			else {
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id,
					text: Localization.GetMessage("CantWithoutNewAcc", user.Lang), showAlert: true);
			}
		}
	}
}
