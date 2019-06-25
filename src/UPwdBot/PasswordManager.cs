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

namespace UPwdBot {
	public static class PasswordManager {
		private const int maxAccsByPage = 3;

		public static int GetAccountCount(int UserId, string accountName) {
			int accountCount;
			if (accountName != null) {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					accountCount = conn.ExecuteScalar<int>(
						"select count(*) from Account where UserId = @UserId and AccountName like @AccountName",
						new {
							UserId,
							AccountName = "%" + accountName.Replace("[", "[[]").Replace("%", "[%]") + "%"
						});
				}
			} else {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					accountCount = conn.ExecuteScalar<int>(
						"select count(*) from Account where UserId = @UserId",
						new {UserId});
				}
			}
			return accountCount;
		}

		public static int GetPageCount(int accountCount) {
			return accountCount % maxAccsByPage == 0 ? accountCount / maxAccsByPage : ((accountCount / maxAccsByPage) + 1);
		}


		/// <param name="chatId"></param>
		/// <param name="accountName">Send null to find all accounts</param>
		/// <param name="langCode"></param>
		/// <returns></returns>
		public static async Task SearchAccounts(int chatId, string accountName, string langCode) {
			int accountCount = GetAccountCount(chatId, accountName);

			if (accountCount == 1) {
				await ShowAccountByName(chatId, accountName, langCode);
			}
			else if (accountCount == 0) {
				await Bot.Instance.Client.SendTextMessageAsync(chatId,
					String.Format(Localization.GetMessage(accountName != null ? "NotFound" : "NoAccounts", langCode), "/add"));
			}
			else if (accountCount <= maxAccsByPage) {
				await ShowSinglePage(chatId, accountName, langCode);
			}
			else {
				await ShowPage(chatId, accountName, 0, GetPageCount(accountCount), langCode);
			}
		}

		private static async Task ShowAccountByName(int UserId, string accountName, string langCode) {
			Account account;
			if (accountName != null) {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					account = conn.QueryFirstOrDefault<Account>(
						"select Id, AccountName, Link, Login from Account where UserId = @UserId and AccountName like @AccountName",
						new {
							UserId,
							AccountName = "%" + accountName.Replace("[", "[[]").Replace("%", "[%]") + "%"
						});
				}
			} else {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					account = conn.QueryFirstOrDefault<Account>(
						"select Id, AccountName, Link, Login from Account where UserId = @UserId",
						new {UserId});
				}
			}
			await ShowAccount(UserId, account, langCode);
		}

		private static string GetPageMessage(List<Account> accounts,
			out InlineKeyboardButton[][] keyboard,
			bool singlePage, string langCode, string message = null) {
			if(message == null)
				message = "";

			keyboard = new InlineKeyboardButton[singlePage ? accounts.Count : accounts.Count + 1][];

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
			return message;
		}

		private static async Task ShowSinglePage(int UserId, string accountName, string langCode) {
			List<Account> accounts;
			if (accountName != null) {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Account where UserId = @UserId and AccountName like @AccountName",
						new {
							UserId,
							AccountName = "%" + accountName.Replace("[", "[[]").Replace("%", "[%]") + "%"
						}).ToList();
				}
			} else {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Account where UserId = @UserId",
						new {UserId}).ToList();
				}
			}

			string message = GetPageMessage(accounts, out InlineKeyboardButton[][] keyboard, true, langCode);

			await Bot.Instance.Client.SendTextMessageAsync(UserId, message,
					replyMarkup: new InlineKeyboardMarkup(keyboard),
					disableWebPagePreview: true);
		}

		public static async Task ShowPage(int UserId,
			string accountName, int page, int pageCount,
			string langCode, int messageToEditId = 0) {

			List<Account> accounts;
			if (accountName != null) {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Account where UserId = @UserId and AccountName like @AccountName " +
							"limit @Limit offset @Offset",
						new {
							UserId,
							AccountName = "%" + accountName.Replace("[", "[[]").Replace("%", "[%]") + "%",
							Limit = maxAccsByPage,
							Offset = page * maxAccsByPage
						})
						.ToList();
				}
			} else {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Account where UserId = @UserId " +
							"limit @Limit offset @Offset",
						new {
							UserId,
							Limit = maxAccsByPage,
							Offset = page * maxAccsByPage
						})
						.ToList();
				}
			}

			string message = Localization.GetMessage("Page", langCode) + " " + (page + 1) + "/" + pageCount + "\n";
			message = GetPageMessage(accounts, out InlineKeyboardButton[][] keyboard, false, langCode, message);

			if (page == 0) {
				keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(
					"▶️ " + Localization.GetMessage("Next",langCode),
					"Q" + 1 + "." + //64 - 6(> + 1 + .) = 58 bytes for search string = 29 chars
					(accountName != null ? 
						accountName.Length <= 29 ? 
						accountName : 
						accountName.Substring(0, 29) : 
						null)) };
			}
			else if (page == pageCount - 1) {
				keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(
					"◀️ " + Localization.GetMessage("Prev",langCode),
					"Q" + (page-1) + "." +
					(accountName != null ? 
						accountName.Length <= page.ToString().Length ? 
						accountName : 
						accountName.Substring(0, page.ToString().Length) : 
						null)) };
			}
			else {
				keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(
					"◀️ " + Localization.GetMessage("Prev",langCode),
					"Q" + (page-1) + "." +
					(accountName != null ? 
						accountName.Length <= page.ToString().Length ? 
						accountName : 
						accountName.Substring(0, page.ToString().Length) : 
						null)),
					InlineKeyboardButton.WithCallbackData(
						"▶️ " + Localization.GetMessage("Next",langCode),
						"Q" + (page+1) + "." +
						(accountName != null ? 
							accountName.Length <= page.ToString().Length ? 
							accountName : 
							accountName.Substring(0, page.ToString().Length) : 
							null))};
			}

			if (messageToEditId == 0) {
				await Bot.Instance.Client.SendTextMessageAsync(UserId, message,
						replyMarkup: new InlineKeyboardMarkup(keyboard),
						disableWebPagePreview: true);
			}
			else {
				await Bot.Instance.Client.EditMessageTextAsync(UserId, messageToEditId, message,
						replyMarkup: new InlineKeyboardMarkup(keyboard),
						disableWebPagePreview: true);
			}
		}

		public static async Task ShowAccount(ChatId chatId, Account account, string langCode, int messageToEditId = 0) {
			if (account != null) {
				string message = account.Link != null ? account.AccountName + "\n" + account.Link + "\n" +
					Localization.GetMessage("Login", langCode) + account.Login :
					account.AccountName + "\n" + Localization.GetMessage("Login", langCode) + account.Login;
				var keyboardMarkup = new InlineKeyboardMarkup(
					new InlineKeyboardButton[][] {
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"🗑 " + Localization.GetMessage("DeleteMsg", langCode),
								"D") },
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"🔑 " + Localization.GetMessage("Password", langCode),
								"P" + account.Id.ToString())}
					});
				if (messageToEditId == 0) {
					await Bot.Instance.Client.SendTextMessageAsync(chatId, message,
						replyMarkup: keyboardMarkup, disableWebPagePreview: true);
				}
				else {
					await Bot.Instance.Client.EditMessageTextAsync(chatId, messageToEditId, message,
						replyMarkup: keyboardMarkup, disableWebPagePreview: true);
				}
			}
		}

	}
}
