using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
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
using Telegram.Bot;

namespace PasswordManager.Bot.Commands; 

public class ShowPasswordCommand : Abstractions.BotCommand, ICallbackQueryCommand, IActionCommand {
	private readonly IAccountService accountService;
	private readonly IPasswordDecryptionService passwordDecryptionService;
	private readonly IUserService userService;
	private readonly ITelegramBotUi telegramBotUi;

	public ShowPasswordCommand(IBot bot,
		IAccountService accountService,
		IPasswordDecryptionService passwordDecryptionService,
		IUserService userService, ITelegramBotUi telegramBotUi)
		: base(bot) {

		this.accountService = accountService;
		this.passwordDecryptionService = passwordDecryptionService;
		this.userService = userService;
		this.telegramBotUi = telegramBotUi;
	}

	async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser botUser) {
		//TODO:
		//Delete answering callbackquery when messages will be edited instead of sending
		await Bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
		long accountId;
		try {
			accountId = Convert.ToInt64(callbackQuery.Data[1..]);
		} catch(Exception exception) {
			//TODO: Log exception
			throw;
		}
		Account account = await accountService.GetPasswordAsync(botUser.Id, accountId);
		if (account != null) {
			if (!account.Encrypted) {
				await Bot.Client.EditMessageTextAsync(botUser.Id, callbackQuery.Message.MessageId,
					telegramBotUi.GetPasswordMessage(account.Password),
					replyMarkup: GetPasswordKeyboard(account, botUser),
					parseMode: ParseMode.MarkdownV2);
			} else {
				passwordDecryptionService.StartDecryptionRequest(botUser.Id, account);
				await userService.UpdateActionAsync(botUser.Id, UserAction.EnterDecryptionKey);
				await Bot.Client.EditMessageTextAsync(botUser.Id, callbackQuery.Message.MessageId,
					"🔑 " + Localization.GetMessage("EnterDecryptionKey", botUser.Lang),
					replyMarkup: GetDecryptionKeyInvitationKeyboard(account, botUser, false));
			}
		}
	}

	//TODO: Move to TelegramBotUi
	private InlineKeyboardMarkup GetDecryptionKeyInvitationKeyboard(Account account, BotUser botUser,
		bool includeShowHintButton) {
		List<List<InlineKeyboardButton>> keyboard = new List<List<InlineKeyboardButton>>();
		if (includeShowHintButton) {
			keyboard.Add(
				new List<InlineKeyboardButton> {
					InlineKeyboardButton.WithCallbackData("💡 " + Localization.GetMessage("ShowHint", botUser.Lang),
						CallbackQueryCommandCode.ShowEncryptionKeyHint.ToStringCode())
				});
		}
		keyboard.Add(
			new List<InlineKeyboardButton> {
				InlineKeyboardButton.WithCallbackData("⬅️ " + Localization.GetMessage("Back", botUser.Lang),
					CallbackQueryCommandCode.ShowAccount.ToStringCode() + account.Id),
				InlineKeyboardButton.WithCallbackData("🛡 " + Localization.GetMessage("UpdatePassword", botUser.Lang),
					UpdateAccountCommandCode.Password.ToStringCode(account.Id))
			});
		keyboard.Add(new List<InlineKeyboardButton> {
			InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", botUser.Lang),
				CallbackQueryCommandCode.DeleteMessage.ToStringCode())
		});
		return new InlineKeyboardMarkup(keyboard);
	}

	//TODO: Move to TelegramBotUi
	private InlineKeyboardMarkup GetPasswordKeyboard(Account account, BotUser botUser) {
		List<List<InlineKeyboardButton>> keyboard = new List<List<InlineKeyboardButton>> {
			new List<InlineKeyboardButton> {
				InlineKeyboardButton.WithCallbackData("⬅️ " + Localization.GetMessage("Back", botUser.Lang),
					CallbackQueryCommandCode.ShowAccount.ToStringCode() + account.Id),
				InlineKeyboardButton.WithCallbackData("🛡 " + Localization.GetMessage("UpdatePassword", botUser.Lang),
					UpdateAccountCommandCode.Password.ToStringCode(account.Id))
			}
		};
			
		var reencryptButton = InlineKeyboardButton.WithCallbackData(
			"🔐 " + Localization.GetMessage(account.Encrypted ? "Reencrypt" : "Encrypt", botUser.Lang),
			CallbackQueryCommandCode.EncryptPassword.ToStringCode() + account.Id);
		var deleteMessageButton = InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", botUser.Lang),
			CallbackQueryCommandCode.DeleteMessage.ToStringCode());
		if (account.Encrypted) {
			var removeEncryptionButton = InlineKeyboardButton.WithCallbackData(
				"🔓 " + Localization.GetMessage("RemoveEncryption", botUser.Lang),
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

	async Task IActionCommand.ExecuteAsync(Message message, BotUser botUser) {
		Account account = passwordDecryptionService.GetAccount(botUser.Id);
		if (account != null) {
			string decryptedPassword = null;
			try {
				decryptedPassword = account.Password.Decrypt(message.Text);
			} catch {
				await Bot.Client.SendTextMessageAsync(botUser.Id,
					Localization.GetMessage("WrongKey", botUser.Lang),
					replyMarkup: GetDecryptionKeyInvitationKeyboard(account, botUser, true),
					parseMode: ParseMode.Markdown);
			}
			passwordDecryptionService.FinishDecryptionRequest(botUser.Id);
			await userService.UpdateActionAsync(botUser.Id, UserAction.Search);
			await Bot.Client.SendTextMessageAsync(botUser.Id, telegramBotUi.GetPasswordMessage(decryptedPassword),
				replyMarkup: GetPasswordKeyboard(account, botUser),
				parseMode: ParseMode.MarkdownV2);
		} else {
			await userService.UpdateActionAsync(botUser.Id, UserAction.Search);
			await Bot.Client.SendTextMessageAsync(message.From.Id,
				Localization.GetMessage("Cancel", botUser.Lang));
		}
	}
}