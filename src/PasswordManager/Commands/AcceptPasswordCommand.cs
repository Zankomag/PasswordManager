using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Types;
using Uten.Encryption;
using Uten.Localization.MultiUser;
using PasswordManager.Types.Enums;

namespace PasswordManager.Commands {
	public class AcceptPasswordCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			if (user.ActionType == UserAction.Assemble) {
				if (PasswordManager.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
					account.Password = callbackQuery.Message.Text.Encrypt();
					PasswordManager.AssemblingAccounts[user.Id] = account;
					await AddAccountCommand.UpdateCallBackMessageAsync(
						callbackQuery.Message.Chat.Id,
						callbackQuery.Message.MessageId,
						account,
						user);
				} else {
					await AnswerWithWarning(callbackQuery.Id, user.Lang);
				}
				return;
			}
			else if(PasswordManager.UpdatingAccounts.ContainsKey(callbackQuery.From.Id)){
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id);
				await PasswordManager.UpdateAccountDataAsync(callbackQuery.Message.Text, PasswordManager.UpdatingAccounts[callbackQuery.From.Id].AccountToUpdateId, callbackQuery.From.Id, user.Lang);
				return;
			}
			await AnswerWithWarning(callbackQuery.Id, user.Lang);
		}

		private async Task AnswerWithWarning(string callbackQueryId, string langCode) {
			await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQueryId,
						text: Localization.GetMessage("CantWithoutNewAcc", langCode), showAlert: true);
		}
	}
}
