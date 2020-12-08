using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PasswordManager.Bot.Types;
using PasswordManager.Bot.Types.Enums;
using PasswordManager.Bot.Extensions;
using MultiUserLocalization;
using Passwords;
using PasswordManager.Core.Entities;
using User = PasswordManager.Core.Entities.User;

namespace PasswordManager.Bot {
	public static class PasswordManagerService {
		public const string separator = "\n──────────────────";
		public const int MinPasswordLength = 1;
		public const int MaxPasswordLength = 2048;
		private const int maxAccsByPage = 3;

		public static Dictionary<int, Account> AssemblingAccounts { get; set; } = new Dictionary<int, Account>();
		public static Dictionary<int, AccountUpdate> UpdatingAccounts { get; set; } = new Dictionary<int, AccountUpdate>();

		public static InlineKeyboardMarkup GeneratePasswordButtonMarkup(string langCode) {
			return new InlineKeyboardMarkup(
						InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("Generate", langCode),
							CallbackCommandCode.GeneratePassword.ToStringCode()));
		}

		//TODO
		//optimize with TotalPages = (int)Math.Ceiling(count / (double)pageSize);
		//https://docs.microsoft.com/en-us/aspnet/core/data/ef-mvc/sort-filter-page?view=aspnetcore-5.0#add-paging-to-students-index
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
				await BotService.Instance.Client.SendTextMessageAsync(chatId,
					String.Format(Localization.GetMessage(accountName != null ? "NotFound" : "NoAccounts", langCode), "/add"));
			}
			else if (accountCount <= maxAccsByPage) {
				await ShowSinglePage(chatId, accountName, langCode);
			}
			else {
				await ShowPage(chatId, accountName, 0, GetPageCount(accountCount), langCode);
			}
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
					CallbackCommandCode.ShowAccount.ToStringCode() + accounts[i].Id.ToString()) };
			}
			return message;
		}

		private static async Task ShowSinglePage(int userId, string accountName, string langCode) {
			List<Account> accounts;
			if (accountName != null) {
				using (IDbConnection conn = new SQLiteConnection(BotService.Instance.connString)) {
					accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Accounts where UserId = @userId and AccountName like @AccountName",
						new {
							userId,
							AccountName = "%" + accountName.Replace("[", "[[]").Replace("%", "[%]") + "%"
						}).ToList();
				}
			} else {
				using (IDbConnection conn = new SQLiteConnection(BotService.Instance.connString)) {
					accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Accounts where UserId = @userId",
						new {userId})
						.ToList();
				}
			}

			string message = GetPageMessage(accounts, out InlineKeyboardButton[][] keyboard, true, langCode);

			await BotService.Instance.Client.SendTextMessageAsync(userId, message,
					replyMarkup: new InlineKeyboardMarkup(keyboard),
					disableWebPagePreview: true);
		}

		public static async Task ShowPage(int userId,
			string accountName, int page, int pageCount,
			string langCode, int messageToEditId = 0) {

			List<Account> accounts;
			if (accountName != null) {
				using (IDbConnection conn = new SQLiteConnection(BotService.Instance.connString)) {
					accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Accounts where UserId = @userId and AccountName like @AccountName " +
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
				using (IDbConnection conn = new SQLiteConnection(BotService.Instance.connString)) {
					accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Accounts where userId = @UserId " +
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
				await BotService.Instance.Client.SendTextMessageAsync(userId, message,
						replyMarkup: new InlineKeyboardMarkup(keyboard),
						disableWebPagePreview: true);
			}
			else {
				await BotService.Instance.Client.EditMessageTextAsync(userId, messageToEditId, message,
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
				CallbackCommandCode.Search.ToStringCode() + (next ? (page + 1).ToString() : (page - 1).ToString()) + 
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
								CallbackCommandCode.ShowPassword.ToStringCode() + account.Id)},
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"✏️ " + Localization.GetMessage("UpdateAcc", langCode),
								CallbackCommandCode.UpdateAccount.ToStringCode() + '0' + account.Id) },
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"🗑 " + Localization.GetMessage("DeleteAcc", langCode),
								CallbackCommandCode.DeleteAccount.ToStringCode() + '0' + account.Id) },
						new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", langCode),
							CallbackCommandCode.DeleteMessage.ToStringCode())
						}
					});
				if(extraMessage != null) {
					message = extraMessage + "\n\n" + message;
				}
				if (messageToEditId == 0) {
					await BotService.Instance.Client.SendTextMessageAsync(chatId, message,
						replyMarkup: keyboardMarkup, disableWebPagePreview: true);
				}
				else {
					await BotService.Instance.Client.EditMessageTextAsync(chatId, messageToEditId, message,
						replyMarkup: keyboardMarkup, disableWebPagePreview: true);
				}
			}
		}

		public static void SetUserPasswordPattern(User user, string passwordPattern = Password.DefaultPasswordGeneratorPattern) {
			if (user != null) {
				using (IDbConnection conn = new SQLiteConnection(BotService.Instance.connString)) {
					conn.Execute("update Users set GenPattern = @passwordPattern where Id = @Id",
						new { passwordPattern, user.Id });
				}
			}
		}

		public static void SetUserLanguage(User user, string langCode) {
			using (IDbConnection conn = new SQLiteConnection(BotService.Instance.connString)) {
				if(user != null) {
					conn.Execute("update Users set Lang = @langCode where Id = @Id",
						new { langCode, user.Id });
				}
			}
		}

		/// <summary>
		/// Checks if user.ActionType != action and writes it to DataBase.
		/// </summary>
		public static void SetUserAction(User user, UserAction action) {
			if (user?.Action != action) {
				SetUserAction(user.Id, action);
			}
		}

		public static void SetUserAction(int userId, UserAction action) {
			using (IDbConnection conn = new SQLiteConnection(BotService.Instance.connString)) {
				conn.Execute("update Users set Action = @action where Id = @userId",
					new { action, userId });
			}
		}

		/// <returns>User that has been added</returns>
		public static User AddUser(int userId, string langCode) {
			using (IDbConnection conn = new SQLiteConnection(BotService.Instance.connString)) {
				conn.Execute("Insert into Users (Id, Lang) values (@userId, @langCode)",
					new { userId, langCode });
			}
			return new User() {
				Id = userId,
				Lang = langCode,
				GenPattern = Password.DefaultPasswordGeneratorPattern,
				Action = UserAction.Search
			};
		}

		public static void DeleteAccountLink(User user, string accountId) {
			using (IDbConnection conn = new SQLiteConnection(BotService.Instance.connString)) {
				if (user != null) {
					conn.Execute("update Accounts set Link = NULL where Id = @accountId and UserId = @Id",
						new { accountId, user.Id });
				}
			}
		}

		public static async Task UpdateAccountAsync(
			ChatId chatId, int messageId, string accountId, string message, 
			string langCode, bool containsDeleteLinkButton, string messageText) {

			string updateCommandCode = CallbackCommandCode.UpdateAccount.ToStringCode();

			InlineKeyboardButton[] accNameButton =
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"📝 " + Localization.GetMessage("AccountName", langCode),
							updateCommandCode + 'N' + accountId)};
			InlineKeyboardButton[] linkButton =
				new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"🔗 " + Localization.GetMessage("Link", langCode),
							updateCommandCode + 'R' + accountId) };
			InlineKeyboardButton[] loginButton =
				new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"📇 " + Localization.GetMessage("Login", langCode),
							updateCommandCode + 'L' + accountId) };
			InlineKeyboardButton[] passwordButton =
				new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"🔐 " + Localization.GetMessage("Password", langCode),
							updateCommandCode + 'P' + accountId) };
			InlineKeyboardButton[] backButton =
				new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"⏪ " + Localization.GetMessage("Back", langCode),
							CallbackCommandCode.ShowAccount.ToStringCode() + accountId) };

			var keyboardMarkup = new InlineKeyboardMarkup(containsDeleteLinkButton ?
				new InlineKeyboardButton[][] {
						accNameButton,
						linkButton,
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"🗑 " + Localization.GetMessage("DeleteLink", langCode),
								updateCommandCode + 'E' + accountId) },
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

			await BotService.Instance.Client.EditMessageTextAsync(chatId,
				messageId,
				message + "\n\n" +
				((messageText.Count(x => x == '\n') > 3) ?
					messageText.Substring(messageText.IndexOf('\n') + 2) :
					messageText),
				replyMarkup: keyboardMarkup,
				disableWebPagePreview: true);
		}

		public static async Task UpdateAccountDataAsync(string data, string accountId, int userId, string langCode) {
			if (UpdatingAccounts.ContainsKey(userId)) {
				AccountUpdate accountUpdate = UpdatingAccounts[userId];
				if(accountUpdate.AccountDataType == AccountDataType.Password) {
					//TODO: ENCRYPT
					data = data.Trim();
				}
				else if(accountUpdate.AccountDataType == AccountDataType.Link) {
					data = data.BuildLink();
				} else {
					data = data.Trim();
				}
				using (IDbConnection conn = new SQLiteConnection(BotService.Instance.connString)) {
					conn.Execute($"update Account set {accountUpdate.AccountDataType.ToString()} = @data where Id = @accountId and UserId = @userId",
						new { data, accountId, userId });
				}
				UpdatingAccounts.Remove(userId);
				SetUserAction(userId, UserAction.Search);
				await ShowAccountById(userId, accountId, langCode, extraMessage: Localization.GetMessage("AccountUpdated", langCode));
				await BotHandlerService.TryDeleteMessageAsync(userId, accountUpdate.MessagetoDeleteId[0]);
				await BotHandlerService.TryDeleteMessageAsync(userId, accountUpdate.MessagetoDeleteId[1]);
			}
		}

		public static async Task<bool> IsLengthExceededAsync(int paramLength, MaxAccountDataLength maxAccountDataLength, int userId, string langCode) {
			if (paramLength > (int)maxAccountDataLength) {
				await ReportExceededLength(userId, langCode, maxAccountDataLength);
				return true;
			}
			return false;
		}

		private static async Task ReportExceededLength(ChatId chatid, string langCode, MaxAccountDataLength maxAccountDataLength) {
			await BotService.Instance.Client.SendTextMessageAsync(chatid,
				String.Format(Localization.GetMessage("MaxLength", langCode), Localization.GetMessage(maxAccountDataLength.ToString(), langCode), (int)maxAccountDataLength));
		}

	}
}
