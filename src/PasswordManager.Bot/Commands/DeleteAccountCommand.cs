using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Commands.Enums;

namespace PasswordManager.Bot.Commands; 

public class DeleteAccountCommand : Abstractions.BotCommand, ICallbackQueryCommand {
	private readonly IAccountService accountService;

	public DeleteAccountCommand(IBot bot, IAccountService accountService) : base(bot) {
		this.accountService = accountService;
	}

	async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser botUser) {
		if (Int64.TryParse(callbackQuery.Data[2..], out long accountId)) {
			if (!String.IsNullOrEmpty(callbackQuery.Data)
				&& callbackQuery.Data[0] == (char)CallbackQueryCommandCode.DeleteAccount) {

				//TODO: move keyboards to ui service
				await (callbackQuery.Data[1] switch {
					(char)DeleteAccountCommandCode.AskForDeletion => (Func<Task>)(async () => {
						var keyboardMarkup = new InlineKeyboardMarkup(
							new InlineKeyboardButton[][] {
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										Localization.GetMessage("ImSure1", botUser.Lang),
										DeleteAccountCommandCode.AskForDeletion2.ToStringCode(accountId))},
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										"❌ " + Localization.GetMessage("No1", botUser.Lang),
										CallbackQueryCommandCode.ShowAccount.ToStringCode() + accountId) },
							});

						await Bot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
							callbackQuery.Message.MessageId,
							Localization.GetMessage("SureDeleteAcc1", botUser.Lang) + "\n\n" + callbackQuery.Message.Text,
							replyMarkup: keyboardMarkup,
							disableWebPagePreview: true);
					}),
					(char)DeleteAccountCommandCode.AskForDeletion2 => (Func<Task>)(async () => {
						var keyboardMarkup = new InlineKeyboardMarkup(
							new InlineKeyboardButton[][] {
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										"❌ " + Localization.GetMessage("No2", botUser.Lang),
										CallbackQueryCommandCode.ShowAccount.ToStringCode() + accountId) },
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										Localization.GetMessage("ImSure2", botUser.Lang),
										DeleteAccountCommandCode.Delete.ToStringCode(accountId))}
							});

						await Bot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
							callbackQuery.Message.MessageId,
							Localization.GetMessage("SureDeleteAcc2", botUser.Lang) + "\n\n" + callbackQuery.Message.Text,
							replyMarkup: keyboardMarkup,
							disableWebPagePreview: true);
					}),
					(char)DeleteAccountCommandCode.Delete => (Func<Task>)(async () => {
						if (await accountService.DeleteAccountAsync(botUser.Id, accountId)) {
							await Bot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
								callbackQuery.Message.MessageId,
								"✅ " + Localization.GetMessage("AccountDeleted", botUser.Lang));
						} else {
							await Bot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
								callbackQuery.Message.MessageId,
								Localization.GetMessage("ErrorTryAgainLater", botUser.Lang));
						}
					}),
					_ => throw new ArgumentException("Unexpected value")
				})();
			}
		}
	}
}