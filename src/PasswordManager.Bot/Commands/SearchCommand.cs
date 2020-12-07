using PasswordManager.Bot.Commands.Abstractions;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Models;

namespace PasswordManager.Bot.Commands {
	public class SearchCommand : IMessageCommand, ICallbackQueryCommand{
		public async Task ExecuteAsync(Message message, BotUser user) {
			await PasswordManagerService.SearchAccounts(message.From.Id, user.Lang, message.Text);
		}

		public async Task ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			int page = Convert.ToInt32(callbackQuery.Data.Substring(1, callbackQuery.Data.IndexOf('.')-1));
			string accountName = callbackQuery.Data.Length != (callbackQuery.Data.IndexOf('.') + 1) ?
				callbackQuery.Data.Substring(callbackQuery.Data.IndexOf('.') + 1) : null;
			int accountCount = PasswordManagerService.GetAccountCount(callbackQuery.From.Id, accountName);
			if(accountCount != 0) {
				await PasswordManagerService.ShowPage(callbackQuery.From.Id, accountName, page,
					PasswordManagerService.GetPageCount(accountCount),
					user.Lang, callbackQuery.Message.MessageId);
				await BotService.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			} else {
				await BotService.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id,
					Localization.GetMessage("SearchAgain", user.Lang), showAlert: true);
				await BotHandler.TryDeleteMessageAsync(
					callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
			}
		}

	}
}
