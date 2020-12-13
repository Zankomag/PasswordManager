using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using MultiUserLocalization;
using PasswordManager.Bot.Extensions;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Application.Encryption;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using System.Text;
using System.Collections.Generic;

namespace PasswordManager.Bot.Commands {
	public class ShowPasswordCommand : Abstractions.BotCommand, ICallbackQueryCommand, IActionCommand {
		private readonly IAccountService accountService;
		private readonly IPasswordDecryptionService passwordDecryptionService;
		private readonly IUserService userService;

		public ShowPasswordCommand(IBot bot,
			IAccountService accountService,
			IPasswordDecryptionService passwordDecryptionService,
			IUserService userService)
			: base(bot) {

			this.accountService = accountService;
			this.passwordDecryptionService = passwordDecryptionService;
			this.userService = userService;
		}

		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			//TODO:
			//Delete answering callbackquery when messages will be edited instead of sending
			await bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			long accountId;
			try {
				accountId = Convert.ToInt64(callbackQuery.Data[1..]);
			} catch(Exception exception) {
				//TODO: Log exception
				throw;
			}
			Account account = await accountService.GetPasswordAsync(user.Id, accountId);
			if (account != null) {
				if (!account.Encrypted) {
					await bot.Client.EditMessageTextAsync(user.Id, callbackQuery.Message.MessageId,
						GetPasswordMessage(account.Password),
						replyMarkup: GetDecryptionKeyInvitationKeyboard(account, user, false, true),
						parseMode: ParseMode.Markdown);
				} else {
					passwordDecryptionService.StartDecryptionRequest(user.Id, account);
					await userService.UpdateActionAsync(user.Id, UserAction.EnterDecryptionKey);
					await bot.Client.EditMessageTextAsync(user.Id, callbackQuery.Message.MessageId,
						"🔑 " + Localization.GetMessage("EnterDecryptionKey", user.Lang),
						replyMarkup: GetDecryptionKeyInvitationKeyboard(account, user, false, false));
				}
			}
		}

		//TODO: Move to BotUIService
		private InlineKeyboardMarkup GetDecryptionKeyInvitationKeyboard(Account account, BotUser user,
			bool showShowHintButton, bool showReencryptButton) {
			List<List<InlineKeyboardButton>> keyboard = new List<List<InlineKeyboardButton>>(); {
				
				
			};
			if (showShowHintButton) {
				keyboard.Add(
					new List<InlineKeyboardButton> {
						InlineKeyboardButton.WithCallbackData("💡 " + Localization.GetMessage("ShowHint", user.Lang),
							CallbackQueryCommandCode.ShowEncryptionKeyHint.ToStringCode())
					});
			}
			keyboard.Add(
				new List<InlineKeyboardButton> {
					InlineKeyboardButton.WithCallbackData("⬅️ " + Localization.GetMessage("Back", user.Lang),
					CallbackQueryCommandCode.ShowAccount.ToStringCode() + account.Id),
					InlineKeyboardButton.WithCallbackData("🛡 " + Localization.GetMessage("Update", user.Lang),
						UpdateAccountCommandCode.Password.ToStringCode() + account.Id)
				}
			);
			List<InlineKeyboardButton> lowerButtons = new List<InlineKeyboardButton>();
			if (showReencryptButton) {
				lowerButtons.Add(InlineKeyboardButton.WithCallbackData(
					"🔐 " + Localization.GetMessage(account.Encrypted ? "Reencrypt" : "Encrypt", user.Lang),
					CallbackQueryCommandCode.EncryptPassword.ToStringCode() + account.Id));
			}
			lowerButtons.Add(InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", user.Lang),
					CallbackQueryCommandCode.DeleteMessage.ToStringCode()));
			keyboard.Add(lowerButtons);
			return new InlineKeyboardMarkup(keyboard);
		}

		private string GetPasswordMessage(string password) 
			=> new StringBuilder('`').Append(password).Append('`').ToString();

		async Task IActionCommand.ExecuteAsync(Message message, BotUser user) {
			Account account = passwordDecryptionService.GetAccount(user.Id);
			if (account != null) {
				string decryptedPassword = null;
				try {
					decryptedPassword = account.Password.Decrypt(message.Text);
				} catch {
					await bot.Client.SendTextMessageAsync(user.Id,
					Localization.GetMessage("WrongKey", user.Lang),
					replyMarkup: GetDecryptionKeyInvitationKeyboard(account, user, true, false),
					parseMode: ParseMode.Markdown);
				}
				await bot.Client.SendTextMessageAsync(user.Id, GetPasswordMessage(decryptedPassword),
					replyMarkup: GetDecryptionKeyInvitationKeyboard(account, user, false, true));
				passwordDecryptionService.FinishDecryptionRequest(user.Id);
			} else {
				await userService.UpdateActionAsync(user.Id, UserAction.Search);
				await bot.Client.SendTextMessageAsync(message.From.Id,
					Localization.GetMessage("Cancel", user.Lang));
			}
		}
	}
}
