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
						replyMarkup: GetPasswordKeyboard(account, user),
						parseMode: ParseMode.MarkdownV2);
				} else {
					passwordDecryptionService.StartDecryptionRequest(user.Id, account);
					await userService.UpdateActionAsync(user.Id, UserAction.EnterDecryptionKey);
					await bot.Client.EditMessageTextAsync(user.Id, callbackQuery.Message.MessageId,
						"🔑 " + Localization.GetMessage("EnterDecryptionKey", user.Lang),
						replyMarkup: GetDecryptionKeyInvitationKeyboard(account, user, false));
				}
			}
		}

		//TODO: Move to BotUIService
		private InlineKeyboardMarkup GetDecryptionKeyInvitationKeyboard(Account account, BotUser user,
			bool includeShowHintButton) {
			List<List<InlineKeyboardButton>> keyboard = new List<List<InlineKeyboardButton>>();
			if (includeShowHintButton) {
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
					InlineKeyboardButton.WithCallbackData("🛡 " + Localization.GetMessage("UpdatePassword", user.Lang),
						UpdateAccountCommandCode.Password.ToStringCode(account.Id))
				});
			keyboard.Add(new List<InlineKeyboardButton> {
				InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", user.Lang),
					CallbackQueryCommandCode.DeleteMessage.ToStringCode())
				});
			return new InlineKeyboardMarkup(keyboard);
		}

		//TODO: Move to BotUIService
		private InlineKeyboardMarkup GetPasswordKeyboard(Account account, BotUser user) {
			List<List<InlineKeyboardButton>> keyboard = new List<List<InlineKeyboardButton>> {
				new List<InlineKeyboardButton> {
					InlineKeyboardButton.WithCallbackData("⬅️ " + Localization.GetMessage("Back", user.Lang),
						CallbackQueryCommandCode.ShowAccount.ToStringCode() + account.Id),
					InlineKeyboardButton.WithCallbackData("🛡 " + Localization.GetMessage("UpdatePassword", user.Lang),
						UpdateAccountCommandCode.Password.ToStringCode(account.Id))
				}
			};
			
			var reencryptButton = InlineKeyboardButton.WithCallbackData(
				"🔐 " + Localization.GetMessage(account.Encrypted ? "Reencrypt" : "Encrypt", user.Lang),
				CallbackQueryCommandCode.EncryptPassword.ToStringCode() + account.Id);
			var deleteMessageButton = InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", user.Lang),
						CallbackQueryCommandCode.DeleteMessage.ToStringCode());
			if (account.Encrypted) {
				var removeEncryptionButton = InlineKeyboardButton.WithCallbackData(
				"🔓 " + Localization.GetMessage("RemoveEncryption", user.Lang),
				UpdateAccountCommandCode.RemoveEncryption.ToStringCode(account.Id));

				keyboard.Add(new List<InlineKeyboardButton> {
					removeEncryptionButton,
					reencryptButton
				});
				keyboard.Add(new List<InlineKeyboardButton> {
					deleteMessageButton
				});
			} else {
				keyboard.Add(new List<InlineKeyboardButton> {
					reencryptButton,
					deleteMessageButton
				});
			}

			return new InlineKeyboardMarkup(keyboard);
		}

		private string GetPasswordMessage(string password) 
			=> new StringBuilder('`')
			.Append(password.EscapeCodeBlockMarkdownV2Chars())
			.Append('`')
			.ToString();

		async Task IActionCommand.ExecuteAsync(Message message, BotUser user) {
			Account account = passwordDecryptionService.GetAccount(user.Id);
			if (account != null) {
				string decryptedPassword = null;
				try {
					decryptedPassword = account.Password.Decrypt(message.Text);
				} catch {
					await bot.Client.SendTextMessageAsync(user.Id,
					Localization.GetMessage("WrongKey", user.Lang),
					replyMarkup: GetDecryptionKeyInvitationKeyboard(account, user, true),
					parseMode: ParseMode.Markdown);
				}
				passwordDecryptionService.FinishDecryptionRequest(user.Id);
				await userService.UpdateActionAsync(user.Id, UserAction.Search);
				await bot.Client.SendTextMessageAsync(user.Id, GetPasswordMessage(decryptedPassword),
					replyMarkup: GetPasswordKeyboard(account, user),
					parseMode: ParseMode.MarkdownV2);
			} else {
				await userService.UpdateActionAsync(user.Id, UserAction.Search);
				await bot.Client.SendTextMessageAsync(message.From.Id,
					Localization.GetMessage("Cancel", user.Lang));
			}
		}
	}
}
