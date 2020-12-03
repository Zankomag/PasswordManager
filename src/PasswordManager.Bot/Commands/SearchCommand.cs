using PasswordManager.Bot.Commands.Abstractions;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using MultiUserLocalization;
using User = PasswordManager.Core.Entities.User;

namespace PasswordManager.Bot.Commands {
	public class SearchCommand : IMessageCommand, ICallBackQueryCommand{
		public async Task ExecuteAsync(Message message, User user) {
			await PasswordManagerHandler.SearchAccounts(message.From.Id, user.Lang, message.Text);
		}

		public async Task ExecuteAsync(CallbackQuery callbackQuery, User user) {
			int page = Convert.ToInt32(callbackQuery.Data.Substring(1, callbackQuery.Data.IndexOf('.')-1));
			string accountName = callbackQuery.Data.Length != (callbackQuery.Data.IndexOf('.') + 1) ?
				callbackQuery.Data.Substring(callbackQuery.Data.IndexOf('.') + 1) : null;
			int accountCount = PasswordManagerHandler.GetAccountCount(callbackQuery.From.Id, accountName);
			if(accountCount != 0) {
				await PasswordManagerHandler.ShowPage(callbackQuery.From.Id, accountName, page,
					PasswordManagerHandler.GetPageCount(accountCount),
					user.Lang, callbackQuery.Message.MessageId);
				await Bot.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			} else {
				await Bot.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id,
					Localization.GetMessage("SearchAgain", user.Lang), showAlert: true);
				await BotHandler.TryDeleteMessageAsync(
					callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
			}
		}

	}
}
