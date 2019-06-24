using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using UPwdBot.Types;

namespace UPwdBot.Commands {
	public class SearchCommand : IMessageCommand, ICallBackQueryCommand{
		private const int maxAccsByPage = 3;
		public async Task ExecuteAsync(Message message, string langCode) {
			
			int accountCount = GetAccountCount(message.From.Id, message.Text);

			if (accountCount == 1) {
				await ShowAccountByName(message.From.Id, message.Text, langCode);
			}
			else if (accountCount == 0) {
				await Bot.Instance.Client.SendTextMessageAsync(message.From.Id,
					String.Format(Localization.GetMessage("NotFound", langCode), "/add"));
			}
			else if (accountCount <= maxAccsByPage) {
				await ShowOnlyPage(message.From.Id, message.Text, langCode);
			}
			else {
				await ShowPageByNum(message.From.Id, message.Text, 0, GetPageCount(accountCount), langCode);
			}
		}

		private int GetAccountCount(int UserId, string accountName) {
			int accountCount;
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				accountCount = conn.ExecuteScalar<int>(
					"select count(*) from Account where UserId = @UserId and AccountName like @AccountName",
					new {
						UserId,
						AccountName = "%" + accountName.Replace("[", "[[]").Replace("%", "[%]") + "%"
					});
			}
			return accountCount;
		}

		private int GetPageCount(int accountCount) {
			return accountCount % maxAccsByPage == 0 ? accountCount / maxAccsByPage : ((accountCount / maxAccsByPage) + 1);
		}

		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			int page = Convert.ToInt32(callbackQuery.Data.Substring(1, callbackQuery.Data.IndexOf('.')-1));
			string accountName = callbackQuery.Data.Substring(callbackQuery.Data.IndexOf('.') + 1);
			int accountCount = GetAccountCount(callbackQuery.From.Id, accountName);
			if(accountCount != 0) {
				await ShowPageByNum(callbackQuery.From.Id, accountName, page, GetPageCount(accountCount), user.Lang, callbackQuery.Message.MessageId);
				await Bot.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			} else {
				await Bot.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id, Localization.GetMessage("SearchAgain", user.Lang), showAlert: true);
				try {
					await Bot.Instance.Client.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
				} catch (Telegram.Bot.Exceptions.ApiRequestException) {
					await Bot.Instance.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId, "🍄");
				}
			}
		}

		private async Task ShowAccountByName(int UserId, string accountName, string langCode) {
			Account account;
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				account = conn.QueryFirstOrDefault<Account>(
					"select Id, AccountName, Link, Login from Account where UserId = @UserId and AccountName like @AccountName",
					new {UserId,
						AccountName = "%" + accountName.Replace("[", "[[]").Replace("%", "[%]") + "%"});
			}
			if(account != null) {
				string message = account.Link != null ? account.AccountName + "\n" + account.Link + "\n" +
					Localization.GetMessage("Login", langCode) + account.Login :
					account.AccountName + "\n" + Localization.GetMessage("Login", langCode) + account.Login;
				await Bot.Instance.Client.SendTextMessageAsync(UserId, message,
					replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", langCode),
								"D") },
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData("🔑 " + Localization.GetMessage("Password", langCode),
								"P" + account.Id.ToString())} }), disableWebPagePreview: true);
			}
		}

		private async Task ShowOnlyPage(int UserId, string accountName, string langCode) {
			List<Account> accounts;
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				accounts = conn.Query<Account>(
					"select Id, AccountName, Link, Login from Account where UserId = @UserId and AccountName like @AccountName",
					new {
						UserId,
						AccountName = "%" + accountName.Replace("[", "[[]").Replace("%", "[%]") + "%"
					}).ToList();
			}

			string message = "";

			InlineKeyboardButton[][] keyboard = new InlineKeyboardButton[accounts.Count][];
			for(int i = 0;i<accounts.Count;i++) {
				if(i!=0)
					message += "\n──────────────────";
				message += "\n" + accounts[i].AccountName;
				if (accounts[i].Link != null)
					message += "\n" + accounts[i].Link;
				message += "\n" + Localization.GetMessage("Login", langCode) + accounts[i].Login;
				keyboard[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(
					(i + 1) + "⃣ " + accounts[i].AccountName,
					"O" + accounts[i].Id.ToString()) };
			}

			await Bot.Instance.Client.SendTextMessageAsync(UserId, message,
					replyMarkup: new InlineKeyboardMarkup(keyboard),
					disableWebPagePreview: true);
		}

		private async Task ShowPageByNum(int UserId, string accountName, int page, int pageCount, string langCode, int editMessageId = 0) {
			List<Account> accounts;
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				accounts = conn.Query<Account>(
					"select Id, AccountName, Link, Login from Account where UserId = @UserId and AccountName like @AccountName " +
						"limit @Limit offset @Offset",
					new {UserId,
						AccountName = "%" + accountName.Replace("[", "[[]").Replace("%", "[%]") + "%",
						Limit = maxAccsByPage,
						Offset = page * maxAccsByPage})
					.ToList();
			}

			string message=  Localization.GetMessage("Page", langCode) + " " +(page+1) + "/" + pageCount + "\n";

			InlineKeyboardButton[][] keyboard = new InlineKeyboardButton[accounts.Count+1][];
			for (int i = 0; i < accounts.Count; i++) {
				if (i != 0)
					message += "\n──────────────────";
				message += "\n" + accounts[i].AccountName;
				if (accounts[i].Link != null)
					message += "\n" + accounts[i].Link;
				message += "\n" + Localization.GetMessage("Login", langCode) + accounts[i].Login;
				keyboard[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(
					(i + 1) + "⃣ " + accounts[i].AccountName,
					"O" + accounts[i].Id.ToString()) };
			}

			if (page == 0) {
				keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(
					"▶️ " + Localization.GetMessage("Next",langCode),
					"Q" + 1 + "." + //64 - 6(> + 1 + .) = 58 bytes for search string = 29 chars
					(accountName.Length <= 29 ? accountName : accountName.Substring(0, 29))) }; 
			} else if (page == pageCount - 1) {
				keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(
					"◀️ " + Localization.GetMessage("Prev",langCode),
					"Q" + (page-1) + "." + 
					(accountName.Length <= page.ToString().Length ? accountName : accountName.Substring(0, page.ToString().Length))) };
			} else {
				keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(
					"◀️ " + Localization.GetMessage("Prev",langCode),
					"Q" + (page-1) + "." +
					(accountName.Length <= page.ToString().Length ? accountName : accountName.Substring(0, page.ToString().Length))),
					InlineKeyboardButton.WithCallbackData(
						"▶️ " + Localization.GetMessage("Next",langCode),
						"Q" + (page+1) + "." +
						(accountName.Length <= page.ToString().Length ? accountName : accountName.Substring(0, page.ToString().Length)))};
			}

			if (editMessageId == 0) {
				await Bot.Instance.Client.SendTextMessageAsync(UserId, message,
						replyMarkup: new InlineKeyboardMarkup(keyboard),
						disableWebPagePreview: true);
			} else {
				await Bot.Instance.Client.EditMessageTextAsync(UserId, editMessageId, message,
						replyMarkup: new InlineKeyboardMarkup(keyboard),
						disableWebPagePreview: true);
			}
		}

	}
}
