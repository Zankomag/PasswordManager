using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Uten.Localization.MultiUser;

namespace UPwdBot.Commands {
	public class SearchCommand : IMessageCommand, ICallBackQueryCommand{
		public async Task ExecuteAsync(Message message, Types.User user) {
			await PasswordManager.SearchAccounts(message.From.Id, user.Lang, message.Text);
		}

		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			int page = Convert.ToInt32(callbackQuery.Data.Substring(1, callbackQuery.Data.IndexOf('.')-1));
			string accountName = callbackQuery.Data.Length != (callbackQuery.Data.IndexOf('.') + 1) ?
				callbackQuery.Data.Substring(callbackQuery.Data.IndexOf('.') + 1) : null;
			int accountCount = PasswordManager.GetAccountCount(callbackQuery.From.Id, accountName);
			if(accountCount != 0) {
				await PasswordManager.ShowPage(callbackQuery.From.Id, accountName, page,
					PasswordManager.GetPageCount(accountCount),
					user.Lang, callbackQuery.Message.MessageId);
				await Bot.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			} else {
				await Bot.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id,
					Localization.GetMessage("SearchAgain", user.Lang), showAlert: true);
				await BotHandler.DeleteMessageAsync(
					callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
			}
		}

	}
}
