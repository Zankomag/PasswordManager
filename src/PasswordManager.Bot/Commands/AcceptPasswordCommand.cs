using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Types.Enums;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Core.Entities;
using User = PasswordManager.Core.Entities.User;
using UserAction = PasswordManager.Core.Entities.User.UserAction;

namespace PasswordManager.Bot.Commands {
	public class AcceptPasswordCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, User user) {
			if (user.Action == UserAction.Assemble) {
				if (PasswordManagerHandler.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
					//TODO:
					//ENCRYPT PASSWORD
					account.Password = callbackQuery.Message.Text;
					PasswordManagerHandler.AssemblingAccounts[user.Id] = account;
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
			else if(PasswordManagerHandler.UpdatingAccounts.ContainsKey(callbackQuery.From.Id)){
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id);
				await PasswordManagerHandler.UpdateAccountDataAsync(callbackQuery.Message.Text, PasswordManagerHandler.UpdatingAccounts[callbackQuery.From.Id].AccountToUpdateId, callbackQuery.From.Id, user.Lang);
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
