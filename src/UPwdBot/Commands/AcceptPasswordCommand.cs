using System.Threading.Tasks;
using Telegram.Bot.Types;
using UPwdBot.Types;
using Uten.Encryption;
using Uten.Localization.MultiUser;

namespace UPwdBot.Commands {
	public class AcceptPasswordCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			if (callbackQuery.Data.Length == 1) {
				if (PasswordManager.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
					account.Password = callbackQuery.Message.Text.Encrypt();
					PasswordManager.AssemblingAccounts[user.Id] = account;
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
			else {
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id);
				PasswordManager.UpdateAccountData(AccountDataTypes.Password, callbackQuery.Message.Text.Encrypt(), callbackQuery.Data.Substring(1), callbackQuery.From.Id);
			}
			
		}
	}
}
