using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MultiUserLocalization;
using System.Linq;
using System.Collections.Generic;
using PasswordManager.Bot.Extensions;
using PasswordManager.Core.Entities;
using User = PasswordManager.Core.Entities.User;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Services.Abstractions;
using System;
using PasswordManager.Bot.Commands.Enums;

namespace PasswordManager.Bot.Commands {
	public class UpdateAccountCommand : Abstractions.BotCommand, ICallbackQueryCommand, IMessageCommand, IActionCommand {
		private readonly IAccountService accountService;

		public UpdateAccountCommand(IBot bot, IAccountService accountService) : base(bot) {
			this.accountService = accountService;
		}

		//TODO:
		//Move all these codes to enum and in other places too
		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			CallbackQueryCommandCode callbackCommandCode;
			try {
				callbackCommandCode = (CallbackQueryCommandCode)callbackQuery.Data[0];
			} catch (Exception exeption) {
				//TODO: Log Exception
				throw;
			}

			switch (callbackCommandCode) {
				//TODO: Refactor
				case CallbackQueryCommandCode.UpdateAccount:
				string accountId = callbackQuery.Data[2..];
					if (callbackQuery.Data[1] == '0') {
						await .UpdateAccountAsync(
							callbackQuery.Message.Chat.Id,
							callbackQuery.Message.MessageId,
							accountId,
							Localization.GetMessage("ChooseWhatUpdate", user.Lang),
							user.Lang,
							callbackQuery.Message.Text.Count(x => x == '\n') % 2 == 0,
							callbackQuery.Message.Text);
					}
					else if(callbackQuery.Data[1] == 'E') {
						//Delete Link
						List<string> accountData = callbackQuery.Message.Text.Split('\n').Skip(2).ToList();
						accountData.RemoveAt(accountData.Count - 2);

						.DeleteAccountLink(user, accountId);
						await .UpdateAccountAsync(
							callbackQuery.Message.Chat.Id,
							callbackQuery.Message.MessageId,
							accountId,
							Localization.GetMessage("LinkDeleted", user.Lang),
							user.Lang,
							false,
							string.Join('\n', accountData));
					}
					else {
						await bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
						.SetUserAction(user, UserAction.UpdateAccount);
						UpdateAccountCommandCode accountDataType = (UpdateAccountCommandCode)(byte)callbackQuery.Data[1];
						InlineKeyboardMarkup inlineKeyboardMarkup = accountDataType == UpdateAccountCommandCode.Password ? 
							.GeneratePasswordButtonMarkup(user.Lang) : 
							null;
						Message sentMessage = await RequestUpdateData(callbackQuery.From.Id, accountDataType.ToString(), user.Lang, inlineKeyboardMarkup);
						.UpdatingAccounts[callbackQuery.From.Id] = new AccountUpdateModel(
							accountId,
							callbackQuery.Message.MessageId,
							sentMessage.MessageId,
							accountDataType);
					}
					break;
				case CallbackQueryCommandCode.AcceptPassword:
					if (.UpdatingAccounts.ContainsKey(callbackQuery.From.Id)) {
						await bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
						await .UpdateAccountDataAsync(
							callbackQuery.Message.Text,
							.UpdatingAccounts[callbackQuery.From.Id].AccountToUpdateId,
							callbackQuery.From.Id,
							user.Lang);
					}
					break;
			}
		}

		async Task IMessageCommand.ExecuteAsync(Message message, BotUser user) {
			if (.UpdatingAccounts.ContainsKey(user.Id)) {
				AccountUpdateModel accountUpdate = .UpdatingAccounts[user.Id];
				if (!await .IsLengthExceededAsync(message.Text.Length, accountUpdate.AccountDataType.ToMaxAccountDataLength(), user.Id, user.Lang)) {
					await .UpdateAccountDataAsync(message.Text, accountUpdate.AccountToUpdateId, user.Id, user.Lang);
				}
			} else {
				.SetUserAction(user, UserAction.Search);
			}
		}

		async Task IActionCommand.ExecuteAsync(Message message, BotUser user) _ERRORR_

		private async Task<Message> RequestUpdateData(ChatId chatId, string dataKey, string langCode, InlineKeyboardMarkup inlineKeyboardMarkup = null) {
			return await bot.Client.SendTextMessageAsync(
				chatId,
				string.Format(Localization.GetMessage("UpdateAccData", langCode), Localization.GetMessage(dataKey, langCode)),
				replyMarkup: inlineKeyboardMarkup);
		}

		//Moved from 
		private async Task UpdateAccountDataAsync(string data, string accountId, int userId, string langCode) {
			if (UpdatingAccounts.ContainsKey(userId)) {
				AccountUpdateModel accountUpdate = UpdatingAccounts[userId];
				if (accountUpdate.AccountDataType == UpdateAccountCommandCode.Password) {
					//TODO: ENCRYPT
					data = data.Trim();
				} else if (accountUpdate.AccountDataType == UpdateAccountCommandCode.Link) {
					data = data.BuildLink();
				} else {
					data = data.Trim();
				}
				using (IDbConnection conn = new SQLiteConnection(bot.connString)) {
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

		//Moved from 
		private async Task UpdateAccountAsync(
			ChatId chatId, int messageId, string accountId, string message,
			string langCode, bool containsDeleteLinkButton, string messageText) {

			string updateCommandCode = CallbackQueryCommandCode.UpdateAccount.ToStringCode();

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
							"⬅️ " + Localization.GetMessage("Back", langCode),
							CallbackQueryCommandCode.ShowAccount.ToStringCode() + accountId) };

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

			await bot.Client.EditMessageTextAsync(chatId,
				messageId,
				message + "\n\n" +
				((messageText.Count(x => x == '\n') > 3) ?
					messageText.Substring(messageText.IndexOf('\n') + 2) :
					messageText),
				replyMarkup: keyboardMarkup,
				disableWebPagePreview: true);
		}

		//Moved from 
		private void DeleteAccountLink(User user, string accountId) {
			using (IDbConnection conn = new SQLiteConnection(bot.connString)) {
				if (user != null) {
					conn.Execute("update Accounts set Link = NULL where Id = @accountId and UserId = @Id",
						new { accountId, user.Id });
				}
			}
		}

	}
}