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
using PasswordManager.Bot.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using System.Text;
using System.Collections.Generic;

namespace PasswordManager.Bot.Commands {
	public class ShowPasswordCommand : Abstractions.BotCommand, ICallbackQueryCommand, IActionCommand {
		private readonly IAccountService accountService;
		private readonly IPasswordDecryptionService passwordDecryptionService;
		private readonly IUserService userService;

		public ShowPasswordCommand(IBotService botService,
			IAccountService accountService,
			IPasswordDecryptionService passwordDecryptionService,
			IUserService userService)
			: base(botService) {

			this.accountService = accountService;
			this.passwordDecryptionService = passwordDecryptionService;
			this.userService = userService;
		}

		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			await botService.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
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
					await botService.Client.EditMessageTextAsync(user.Id, callbackQuery.Message.MessageId,
						GetPasswordMessage(account.Password),
						replyMarkup: GetDecryptionKeyInvitationKeyboard(accountId, user, false),
						parseMode: ParseMode.Markdown);
				} else {
					await userService.UpdateActionAsync(user.Id, UserAction.EnterDecryptionKey);
					await botService.Client.EditMessageTextAsync(user.Id, callbackQuery.Message.MessageId,
						"🔑 " + Localization.GetMessage("EnterDecryptionKey", user.Lang),
						replyMarkup: GetDecryptionKeyInvitationKeyboard(accountId, user, false));
				}
			}
		}

		//TODO: Move to BotUIService
		private InlineKeyboardMarkup GetDecryptionKeyInvitationKeyboard(long accountId, BotUser user, bool showShowHintButton) {
			List<InlineKeyboardButton[]> keyboard = new List<InlineKeyboardButton[]> {
				new InlineKeyboardButton[] {
					InlineKeyboardButton.WithCallbackData("⬅️ " + Localization.GetMessage("Back", user.Lang),
						CallbackQueryCommandCode.ShowAccount.ToStringCode() + accountId)
				}
			};
			if (showShowHintButton) {
				keyboard.Add(
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData("💡 " + Localization.GetMessage("ShowHint", user.Lang),
							CallbackQueryCommandCode.ShowEncryptionKeyHint.ToStringCode()) 
					});
			}
			keyboard.Add(
				new InlineKeyboardButton[] {
				InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", user.Lang),
					CallbackQueryCommandCode.DeleteMessage.ToStringCode())
				});
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
					await botService.Client.SendTextMessageAsync(user.Id,
					Localization.GetMessage("WrongKey", user.Lang),
					replyMarkup: GetDecryptionKeyInvitationKeyboard(account.Id, user, true),
					parseMode: ParseMode.Markdown);
				}
				await botService.Client.SendTextMessageAsync(user.Id, GetPasswordMessage(decryptedPassword));
				passwordDecryptionService.FinishDecryptionRequest(user.Id);
			} else {
				await userService.UpdateActionAsync(user.Id, UserAction.Search);
				await botService.Client.SendTextMessageAsync(message.From.Id,
					Localization.GetMessage("Cancel", user.Lang));
			}
		}
	}
}
