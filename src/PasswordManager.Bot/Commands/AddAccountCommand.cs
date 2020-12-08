using System;
using System.Data;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using PasswordManager.Bot;
using MultiUserLocalization;
using PasswordManager.Bot.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Application.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class AddAccountCommand : Abstractions.BotCommand, IMessageCommand, IActionCommand {
		//Moved from 
		public const int MinPasswordLength = 1;
		//Moved from 
		public const int MaxPasswordLength = 2048;
		private readonly IAccountService accountService;

		public AddAccountCommand(IBotService botService, IAccountService accountService) : base(botService) {
			this.accountService = accountService;
		}

		async Task IMessageCommand.ExecuteAsync(Message message, BotUser user) {
			if (message.Text.StartsWith("/add") && message.Text.Length > 5) {
				.AssemblingAccounts.Remove(message.From.Id);
				await AssembleAccountAsync(message, user);
			}
			else if (message.Text == "/add") {
				.AssemblingAccounts[message.From.Id] = new Account() { UserId = message.From.Id };
				.SetUserAction(user, UserAction.AssembleAccount);
				await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
					"📝 " + Localization.GetMessage("AddAccount", user.Lang));
			}
			//then user is already assembling account, so CONTINUE assembling it
			else {
				Account account = .AssemblingAccounts[message.From.Id];
				string data = !message.Text.Contains('\n') ? message.Text :
					message.Text.Substring(0, message.Text.IndexOf('\n'));
				if (account.AccountName == null) {
					if (!await .IsLengthExceededAsync(message.Text.Length, MaxAccountDataLength.AccountName, message.From.Id, user.Lang)) {
						account.AccountName = data.Trim();
						.AssemblingAccounts[message.From.Id] = account;
						.SetUserAction(user, UserAction.AssembleAccount);
						await AddLinkPrompt(message.Chat.Id, account.AccountName, user.Lang);
					}
					//TODO
					//ADD SKIPLINK CKECK
					//CREATE SPECIAL MODEL FOR ADDING NEW ACCOUNT WITH SKIP LINK FIELD
				} else if(/*!account.SkipLink &&*/ account.Link == null){
					if (!await .IsLengthExceededAsync(message.Text.Length,MaxAccountDataLength.Link, message.From.Id, user.Lang)) {
						account.Link = data.BuildLink();
						.AssemblingAccounts[message.From.Id] = account;
						.SetUserAction(user, UserAction.AssembleAccount);
						if (account.Login == null) {
							await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
								"📇 " + Localization.GetMessage("AddLogin", user.Lang));
						}
						else {
							await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
							"🔐 " + String.Format(Localization.GetMessage("AddPassword", user.Lang), "/generator"),
						replyMarkup: .GeneratePasswordButtonMarkup(user.Lang));
						}
					}
				} else if (account.Login == null) {
					if (!await .IsLengthExceededAsync(message.Text.Length, MaxAccountDataLength.Login, message.From.Id, user.Lang)) {
						account.Login = data.Trim();
						.AssemblingAccounts[message.From.Id] = account;
						.SetUserAction(user, UserAction.AssembleAccount);
						await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
							"🔐 " + String.Format(Localization.GetMessage("AddPassword", user.Lang), "/generator"),
						replyMarkup: .GeneratePasswordButtonMarkup(user.Lang));
					}
				} else if (account.Password == null) {
					if (!await .IsLengthExceededAsync(message.Text.Length, MaxAccountDataLength.Password, message.From.Id, user.Lang)) {
						//TODO: ENCRYPT PASSWORD
						account.Password = data.Trim();
						await SaveToDBAsync(account, user);
						.AssemblingAccounts.Remove(message.From.Id);
					}
				}
			}
		}

		private async Task AssembleAccountAsync(Message message, User user) {
			string[] accountData = message.Text.Remove(0, 5).Split('\n', StringSplitOptions.RemoveEmptyEntries);
			if (await .IsLengthExceededAsync(accountData[0].Length, MaxAccountDataLength.AccountName, message.From.Id, user.Lang)) {
				return;
			}
			Account account = new Account {
				AccountName = accountData[0].Trim(),
				UserId = message.From.Id
			};

			if (accountData.Length > 3) {
				if (!await .IsLengthExceededAsync(accountData[1].Length, MaxAccountDataLength.Link,		message.From.Id, user.Lang) &&
					!await .IsLengthExceededAsync(accountData[2].Length, MaxAccountDataLength.Login,		message.From.Id, user.Lang) &&
					!await .IsLengthExceededAsync(accountData[3].Length, MaxAccountDataLength.Password,	message.From.Id, user.Lang)) {

					account.Link = accountData[1].BuildLink();
					account.Login = accountData[2].Trim();
					//TODO: ENCRYPT PASSWORD
					account.Password = accountData[3].Trim();
					await SaveToDBAsync(account, user);
				}
			} else if(accountData.Length == 3) {
				if (!await .IsLengthExceededAsync(accountData[2].Length, MaxAccountDataLength.Password,	message.From.Id, user.Lang) &&
					!await .IsLengthExceededAsync(accountData[1].Length, MaxAccountDataLength.Login,		message.From.Id, user.Lang)) {
					
					account.Login = accountData[1].Trim();
					//TODO: ENCRYPT PASSWORD
					account.Password = accountData[2].Trim();
					await SaveToDBAsync(account, user);
				}
			} else if(accountData.Length == 2) {
				if (!await .IsLengthExceededAsync(accountData[1].Length, MaxAccountDataLength.Login, message.From.Id, user.Lang)) {
					account.Login = accountData[1].Trim();
					.AssemblingAccounts[message.From.Id] = account;
					.SetUserAction(user, UserAction.AssembleAccount);
					await AddLinkPrompt(message.Chat.Id, account.AccountName, user.Lang);
				}
			} else {
				.AssemblingAccounts[message.From.Id] = account;
				.SetUserAction(user, UserAction.AssembleAccount);
				await AddLinkPrompt(message.Chat.Id, account.AccountName, user.Lang);
			}

		}

		private static async Task SaveToDBAsync(Account account, User user, int messageToEditId = 0) {
			using (IDbConnection conn = new SQLiteConnection(botService.connString)) {
				account.Id = (long)conn.ExecuteScalar("Insert into Accounts (UserId, AccountName, Link, Login, Password) " +
					"values (@UserId, @AccountName, @Link, @Login, @Password);" +
					"select last_insert_rowid()",
					account);
			}

			.SetUserAction(user, UserAction.Search);

			await .ShowAccount(account.UserId, account, user.Lang, messageToEditId: messageToEditId,
				extraMessage: "✅ " + String.Format(Localization.GetMessage("AccountAdded", user.Lang), account.AccountName));
		}

		private static async Task AddLinkPrompt(ChatId chatId, string accountName, string langCode) {
			var inlineKeyBoard = new InlineKeyboardMarkup(
				new InlineKeyboardButton[][] {
							new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData("🔗 " + accountName.AutoLink(), CallbackQueryCommandCode.AutoLink.ToStringCode())
							},
							new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData(
									"⏩ " + Localization.GetMessage("Skip",langCode), CallbackQueryCommandCode.SkipLink.ToStringCode())
							}
				}
			);
			await BotHandler.Bot.SendTextMessageAsync(chatId,
				"🔗 " + String.Format(Localization.GetMessage("AddLink", langCode), "/help"),
				replyMarkup: inlineKeyBoard);
		}

		public static async Task UpdateCallBackMessageAsync(ChatId chatId, int messageId, Account account, User user) {

			if (account.AccountName == null) {
				await BotHandler.Bot.EditMessageTextAsync(chatId, messageId,
					"📝 " + Localization.GetMessage("AddAccount", user.Lang));
			}
			//TODO
			//ADD SKIPLINK CKECK
			else if (/*!account.SkipLink &&*/ account.Link == null) {
				await AddLinkPrompt(chatId, account.AccountName, user.Lang);
			}
			else if (account.Login == null) {
				await BotHandler.Bot.EditMessageTextAsync(chatId, messageId,
					"📇 " + Localization.GetMessage("AddLogin", user.Lang));
			}
			else if (account.Password == null) {
				await BotHandler.Bot.EditMessageTextAsync(chatId, messageId,
					"🔐 " + String.Format(Localization.GetMessage("AddPassword", user.Lang), "/generator"),
					replyMarkup: .GeneratePasswordButtonMarkup(user.Lang));
			}
			else {
				.AssemblingAccounts.Remove(account.UserId);
				await SaveToDBAsync(account, user, messageId);
			}
		}

		//Moved from 
		private async Task<bool> IsLengthExceededAsync(int paramLength, MaxAccountDataLength maxAccountDataLength, int userId, string langCode) {
			if (paramLength > (int)maxAccountDataLength) {
				await ReportExceededLength(userId, langCode, maxAccountDataLength);
				return true;
			}
			return false;
		}

		//Moved from 
		private async Task ReportExceededLength(ChatId chatid, string langCode, MaxAccountDataLength maxAccountDataLength) {
			await botService.Client.SendTextMessageAsync(chatid,
				String.Format(Localization.GetMessage("MaxLength", langCode), Localization.GetMessage(maxAccountDataLength.ToString(), langCode), (int)maxAccountDataLength));
		}

		//Moved from 
		private async Task ShowAccount(ChatId chatId, Account account, string langCode, int messageToEditId = 0, string extraMessage = null) {
			if (account != null) {
				string message = account.Link != null ? account.AccountName + "\n" + account.Link + "\n" +
					Localization.GetMessage("Login", langCode) + ": " + account.Login :
					account.AccountName + "\n" + Localization.GetMessage("Login", langCode) + ": " + account.Login;
				var keyboardMarkup = new InlineKeyboardMarkup(
					new InlineKeyboardButton[][] {
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"🔑 " + Localization.GetMessage("Password", langCode),
								CallbackQueryCommandCode.ShowPassword.ToStringCode() + account.Id)},
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"✏️ " + Localization.GetMessage("UpdateAcc", langCode),
								CallbackQueryCommandCode.UpdateAccount.ToStringCode() + '0' + account.Id) },
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"🗑 " + Localization.GetMessage("DeleteAcc", langCode),
								CallbackQueryCommandCode.DeleteAccount.ToStringCode() + '0' + account.Id) },
						new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", langCode),
							CallbackQueryCommandCode.DeleteMessage.ToStringCode())
						}
					});
				if (extraMessage != null) {
					message = extraMessage + "\n\n" + message;
				}
				if (messageToEditId == 0) {
					await botService.Client.SendTextMessageAsync(chatId, message,
						replyMarkup: keyboardMarkup, disableWebPagePreview: true);
				} else {
					await botService.Client.EditMessageTextAsync(chatId, messageToEditId, message,
						replyMarkup: keyboardMarkup, disableWebPagePreview: true);
				}
			}
		}

		async Task IActionCommand.ExecuteAsync(Message message, BotUser user) => _eRROR_ throw new NotImplementedException();
	}
}
