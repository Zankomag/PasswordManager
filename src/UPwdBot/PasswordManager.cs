using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using UPwdBot.Types;
using Uten.Localization.MultiUser;
using Uten.Passwords;

namespace UPwdBot {
	public static class PasswordManager {
		public const string separator = "\n──────────────────";
		private const int maxAccsByPage = 3;

		public static Dictionary<int, Account> AssemblingAccounts { get; set; } = new Dictionary<int, Account>();
		public static Dictionary<int, Updates> UpdatingAccounts { get; set; } = new Dictionary<int, Updates>();

		public static int GetAccountCount(int UserId, string accountName = null) {
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
		public static async Task SearchAccounts(int chatId, string langCode, string accountName = null) {

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

		private static async Task ShowAccountByName(int userId, string accountName, string langCode) {
			Account account;
			if (accountName != null) {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					account = conn.QueryFirstOrDefault<Account>(
						"select Id, AccountName, Link, Login from Account where UserId = @userId and AccountName like @AccountName",
						new {
							userId,
							AccountName = "%" + accountName.Replace("[", "[[]").Replace("%", "[%]") + "%"
						});
				}
			} else {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					account = conn.QueryFirstOrDefault<Account>(
						"select Id, AccountName, Link, Login from Account where UserId = @userId",
						new {userId});
				}
			}
			await ShowAccount(userId, account, langCode);
		}

		private static string GetPageMessage(List<Account> accounts,
			out InlineKeyboardButton[][] keyboard,
			bool singlePage, string langCode, string message = null) {
			if(message == null)
				message = "";

			keyboard = new InlineKeyboardButton[singlePage ? accounts.Count : accounts.Count + 1][];

			for (int i = 0; i < accounts.Count; i++) {
				if (i != 0)
					message += separator;
				message += "\n" + accounts[i].AccountName;
				if (accounts[i].Link != null)
					message += "\n" + accounts[i].Link;
				message += "\n" + Localization.GetMessage("Login", langCode) + ": " + accounts[i].Login;
				keyboard[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(
					(i + 1) + "⃣ " + accounts[i].AccountName,
					"O" + accounts[i].Id.ToString()) };
			}
			return message;
		}

		private static async Task ShowSinglePage(int userId, string accountName, string langCode) {
			List<Account> accounts;
			if (accountName != null) {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Account where UserId = @userId and AccountName like @AccountName",
						new {
							userId,
							AccountName = "%" + accountName.Replace("[", "[[]").Replace("%", "[%]") + "%"
						}).ToList();
				}
			} else {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Account where UserId = @userId",
						new {userId})
						.ToList();
				}
			}

			string message = GetPageMessage(accounts, out InlineKeyboardButton[][] keyboard, true, langCode);

			await Bot.Instance.Client.SendTextMessageAsync(userId, message,
					replyMarkup: new InlineKeyboardMarkup(keyboard),
					disableWebPagePreview: true);
		}

		public static async Task ShowPage(int userId,
			string accountName, int page, int pageCount,
			string langCode, int messageToEditId = 0) {

			List<Account> accounts;
			if (accountName != null) {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Account where UserId = @userId and AccountName like @AccountName " +
							"limit @maxAccsByPage offset @Offset",
						new {
							userId,
							AccountName = "%" + accountName.Replace("[", "[[]").Replace("%", "[%]") + "%",
							maxAccsByPage,
							Offset = page * maxAccsByPage
						})
						.ToList();
				}
			} else {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Account where userId = @UserId " +
							"limit @maxAccsByPage offset @Offset",
						new {
							userId,
							maxAccsByPage,
							Offset = page * maxAccsByPage
						})
						.ToList();
				}
			}

			string message = Localization.GetMessage("Page", langCode) + " " + (page + 1) + "/" + pageCount + "\n";
			message = GetPageMessage(accounts, out InlineKeyboardButton[][] keyboard, false, langCode, message);

			//This is first page
			if (page == 0) {
				keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] {
					GetPageButton(true, page, accountName, langCode)};
			}
			//This is last page
			else if (page == pageCount - 1) {
				keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] {
					GetPageButton(false, page, accountName, langCode)};
			}
			//This is middle page
			else {
				keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] {
					GetPageButton(false, page, accountName, langCode),
					GetPageButton(true, page, accountName, langCode)};
			}

			if (messageToEditId == 0) {
				await Bot.Instance.Client.SendTextMessageAsync(userId, message,
						replyMarkup: new InlineKeyboardMarkup(keyboard),
						disableWebPagePreview: true);
			}
			else {
				await Bot.Instance.Client.EditMessageTextAsync(userId, messageToEditId, message,
						replyMarkup: new InlineKeyboardMarkup(keyboard),
						disableWebPagePreview: true);
			}
		}

		public static InlineKeyboardButton GetPageButton(bool next, int page, string accountName, string langCode) {
			if (accountName != null) {
				//Telegram inline button accepts only 64 bytes of data. UTF-16 string has 2 bytes per char.
				//So, search string can be no more than 64 - (4(>(2 bytes) + . (2 bytes)) + page.ToString().Length*2) chars
				int maxLength = (64 - (4/*(> + .)*/ + page.ToString().Length*2));
				accountName = accountName.Length <= maxLength ?
					accountName : accountName.Substring(0, maxLength);
			}
			return InlineKeyboardButton.WithCallbackData(
				next ? "▶️ " + Localization.GetMessage("Next", langCode) :
					"◀️ " + Localization.GetMessage("Prev", langCode),
				"Q" + (next ? (page + 1).ToString() : (page - 1).ToString()) + 
				"." + accountName);
		}

		public static async Task ShowAccount(ChatId chatId, Account account, string langCode, int messageToEditId = 0, string extraMessage = null) {
			if (account != null) {
				string message = account.Link != null ? account.AccountName + "\n" + account.Link + "\n" +
					Localization.GetMessage("Login", langCode) + ": " + account.Login :
					account.AccountName + "\n" + Localization.GetMessage("Login", langCode) + ": " + account.Login;
				var keyboardMarkup = new InlineKeyboardMarkup(
					new InlineKeyboardButton[][] {
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"🔑 " + Localization.GetMessage("Password", langCode),
								"P" + account.Id)},
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"✏️ " + Localization.GetMessage("UpdateAcc", langCode),
								"U0" + (account.Link != null ? '1' : '0') + account.Id) },
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"🗑 " + Localization.GetMessage("DeleteAcc", langCode),
								"X0" + account.Id) },
					});
				if(extraMessage != null) {
					message = extraMessage + "\n\n" + message;
				}
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

		public static async Task ShowAccountById(int userId, string accountId, string langCode, int messageToEditId = 0) {
			Account account;
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				account = conn.QueryFirstOrDefault<Account>(
					"select Id, AccountName, Link, Login from Account where Id = @accountId and UserId = @userId",
					new {
						accountId,
						userId
					});
			}
			await ShowAccount(userId, account, langCode, messageToEditId);
		}

		public static void SetUserPasswordPattern(Types.User user, string passwordPattern = Password.defaultPasswordGeneratorPattern) {
			if (user != null) {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					conn.Execute("update User set GenPattern = @passwordPattern where Id = @Id",
						new { passwordPattern, user.Id });
				}
			}
		}

		public static void SetUserLanguage(Types.User user, string langCode) {
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				if(user != null) {
					conn.Execute("update User set Lang = @langCode where Id = @Id",
						new { langCode, user.Id });
				}
			}
		}

		public static void SetUserAction(Types.User user, Actions action) {
			if (user?.ActionType != action) {
				using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
					conn.Execute("update User set Action = @action where Id = @Id",
						new { action, user.Id });
				}
			}
		}

		/// <returns>User that has been added</returns>
		public static Types.User AddUser(int userId, string langCode) {
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				conn.Execute("Insert into User (Id, Lang) values (@userId, @langCode)",
					new { userId, langCode });
			}
			return new Types.User() {
				Id = userId,
				Lang = langCode,
				GenPattern = Password.defaultPasswordGeneratorPattern,
				ActionType = Types.User.DefaultAction
			};
		}

		public static void DeleteAccountLink(Types.User user, string accountId) {
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				if (user != null) {
					conn.Execute("update Account set Link = NULL where Id = @accountId and UserId = @Id",
						new { accountId, user.Id });
				}
			}
		}

		public static async Task UpdateAccountAsync(
			ChatId chatId, int messageId, string accountId, string message, 
			string langCode, char containsDeleteLinkButton, string messageText) {

			InlineKeyboardButton[] accNameButton =
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"📝 " + Localization.GetMessage("AccountName", langCode),
							"UN" + containsDeleteLinkButton + accountId)};
			InlineKeyboardButton[] linkButton =
				new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"🔗 " + Localization.GetMessage("Link", langCode),
							"UR" + containsDeleteLinkButton + accountId) };
			InlineKeyboardButton[] loginButton =
				new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"📇 " + Localization.GetMessage("Login", langCode),
							"UL" + containsDeleteLinkButton + accountId) };
			InlineKeyboardButton[] passwordButton =
				new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"🔐 " + Localization.GetMessage("Password", langCode),
							"UP" + containsDeleteLinkButton + accountId) };
			InlineKeyboardButton[] backButton =
				new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"⏪ " + Localization.GetMessage("Back", langCode),
							"O" + accountId) };

			var keyboardMarkup = new InlineKeyboardMarkup(containsDeleteLinkButton == '1' ?
				new InlineKeyboardButton[][] {
						accNameButton,
						linkButton,
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"🗑 " + Localization.GetMessage("DeleteLink", langCode),
								"UE" + containsDeleteLinkButton + accountId) },
						loginButton,
						passwordButton,
						backButton
				} :
				new InlineKeyboardButton[][] {
						accNameButton,
						linkButton,
						loginButton,
						passwordButton,
						backButton});

			await Bot.Instance.Client.EditMessageTextAsync(chatId,
				messageId,
				"*" + message + "* \n\n" +
				((messageText.Count(x => x == '\n') > 3) ?
					messageText.Substring(messageText.IndexOf('\n') + 2) :
					messageText),
				replyMarkup: keyboardMarkup,
				parseMode: ParseMode.Markdown,
				disableWebPagePreview: true);
		}

	}
}
