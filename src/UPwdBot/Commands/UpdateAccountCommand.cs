using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace UPwdBot.Commands {
	public class UpdateAccountCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			switch (callbackQuery.Data[1]) {
				case '0': {
					string accountId = callbackQuery.Data.Substring(3);
					var keyboardMarkup = callbackQuery.Data[2] == '1' ?
						new InlineKeyboardMarkup(
							new InlineKeyboardButton[][] {
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										"📝 " + Localization.GetMessage("AccountName", user.Lang),
										"UN" + accountId)},
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										"🔗 " + Localization.GetMessage("Link", user.Lang),
										"UR" + accountId) },
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										"🗑 " + Localization.GetMessage("DeleteLink", user.Lang),
										"UE" + accountId) },
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										"📇 " + Localization.GetMessage("Login", user.Lang),
										"UL" + accountId) },
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										"🔐 " + Localization.GetMessage("Password", user.Lang),
										"UP" + accountId) },
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										"⏪ " + Localization.GetMessage("Back", user.Lang),
										"O" + accountId) },
							}) : 
							new InlineKeyboardMarkup(
								new InlineKeyboardButton[][] {
									new InlineKeyboardButton[] {
										InlineKeyboardButton.WithCallbackData(
											"📝 " + Localization.GetMessage("AccountName", user.Lang),
											"UN" + accountId)},
									new InlineKeyboardButton[] {
										InlineKeyboardButton.WithCallbackData(
											"🔗 " + Localization.GetMessage("Link", user.Lang),
											"UR" + accountId) },
									new InlineKeyboardButton[] {
										InlineKeyboardButton.WithCallbackData(
											"📇 " + Localization.GetMessage("Login", user.Lang),
											"UL" + accountId) },
									new InlineKeyboardButton[] {
										InlineKeyboardButton.WithCallbackData(
											"🔐 " + Localization.GetMessage("Password", user.Lang),
											"UP" + accountId) },
									new InlineKeyboardButton[] {
										InlineKeyboardButton.WithCallbackData(
											"⏪ " + Localization.GetMessage("Back", user.Lang),
											"O" + accountId) },
								});

					await Bot.Instance.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
						callbackQuery.Message.MessageId,
						"**" + Localization.GetMessage("ChooseWhatUpdate", user.Lang) + "**\n\n" + callbackQuery.Message.Text,
						replyMarkup: keyboardMarkup,
						parseMode: ParseMode.Markdown,
						disableWebPagePreview: true);
					break;
				}
				case 'N': {
					//Account Name

					break;
				}
				case 'R': {
					//Link

					break;
				}
				case 'L': {
					//Login

					break;
				}
				case 'P': {
					//Password

					break;
				}
				case 'E': {
					//delete Link
					break;
				}
			}
		}
	}
}