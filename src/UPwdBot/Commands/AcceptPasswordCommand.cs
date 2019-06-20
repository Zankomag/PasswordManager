using System.Threading.Tasks;
using Telegram.Bot.Types;
using UPwdBot.Types;
using Uten.Encryption;

namespace UPwdBot.Commands {
	public class AcceptPasswordCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			if (BotHandler.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
				account.Password = callbackQuery.Message.Text.Encrypt();
				BotHandler.AssemblingAccounts[user.Id] = account;
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id);
				await AddAccountCommand.UpdateCallBackMessageAsync(
					callbackQuery.Message.Chat.Id,
					callbackQuery.Message.MessageId,
					account,
					user.Lang);
			}
			else {
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id,
					text: Localization.GetMessage("CantWithoutNewAcc", user.Lang), showAlert: true);
			}
		}
	}
}
