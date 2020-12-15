using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MultiUserLocalization;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Commands.Enums;

namespace PasswordManager.Bot.Commands {
	public class DeleteAccountCommand : Abstractions.BotCommand, ICallbackQueryCommand {
		private readonly IAccountService accountService;

		public DeleteAccountCommand(IBot bot, IAccountService accountService) : base(bot) {
			this.accountService = accountService;
		}

		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			if (long.TryParse(callbackQuery.Data[2..], out long accountId)) {
				if (!string.IsNullOrEmpty(callbackQuery.Data)
					&& callbackQuery.Data[0] == (char)CallbackQueryCommandCode.DeleteAccount) {

					//TODO: move keyboards to ui service
					await (callbackQuery.Data[1] switch {
						(char)DeleteAccountCommandCode.AskForDeletion => (Func<Task>)(async () => {
							var keyboardMarkup = new InlineKeyboardMarkup(
							new InlineKeyboardButton[][] {
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										Localization.GetMessage("ImSure1", user.Lang),
										DeleteAccountCommandCode.AskForDeletion2.ToStringCode(accountId))},
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										"❌ " + Localization.GetMessage("No1", user.Lang),
										CallbackQueryCommandCode.ShowAccount.ToStringCode() + accountId) },
							});

							await bot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
									callbackQuery.Message.MessageId,
									Localization.GetMessage("SureDeleteAcc1", user.Lang) + "\n\n" + callbackQuery.Message.Text,
									replyMarkup: keyboardMarkup,
									disableWebPagePreview: true);
						}),
						(char)DeleteAccountCommandCode.AskForDeletion2 => (Func<Task>)(async () => {
							var keyboardMarkup = new InlineKeyboardMarkup(
							new InlineKeyboardButton[][] {
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										"❌ " + Localization.GetMessage("No2", user.Lang),
										CallbackQueryCommandCode.ShowAccount.ToStringCode() + accountId) },
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										Localization.GetMessage("ImSure2", user.Lang),
										DeleteAccountCommandCode.Delete.ToStringCode(accountId))}
							});

							await bot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
									callbackQuery.Message.MessageId,
									Localization.GetMessage("SureDeleteAcc2", user.Lang) + "\n\n" + callbackQuery.Message.Text,
									replyMarkup: keyboardMarkup,
									disableWebPagePreview: true);
						}),
						(char)DeleteAccountCommandCode.Delete => (Func<Task>)(async () => {
							if (await accountService.DeleteAccountAsync(user.Id, accountId)) {
								await bot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
									callbackQuery.Message.MessageId,
									"✅ " + Localization.GetMessage("AccountDeleted", user.Lang));
							} else {
								await bot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
									callbackQuery.Message.MessageId,
									Localization.GetMessage("ErrorTryAgainLater", user.Lang));
							}
						}),
						_ => throw new ArgumentException("Unexpected value")
					})();
				}
			}
		}
	}
}
