using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Uten.Localization.MultiUser;

namespace UPwdBot.Commands {
	public class UpdateAccountCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			string accountId = callbackQuery.Data.Substring(3);
			if (callbackQuery.Data[1] == '0') {
				InlineKeyboardButton[] accNameButton =
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"📝 " + Localization.GetMessage("AccountName", user.Lang),
							"UN" + accountId)};
				InlineKeyboardButton[] linkButton = 
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"🔗 " + Localization.GetMessage("Link", user.Lang),
							"UR" + accountId) };
				InlineKeyboardButton[] loginButton = 
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
								"📇 " + Localization.GetMessage("Login", user.Lang),
								"UL" + accountId) };
				InlineKeyboardButton[] passwordButton = 
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"🔐 " + Localization.GetMessage("Password", user.Lang),
							"UP" + accountId) };
				InlineKeyboardButton[] backButton = 
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"⏪ " + Localization.GetMessage("Back", user.Lang),
							"O" + accountId) };
				var keyboardMarkup = new InlineKeyboardMarkup(callbackQuery.Data[2] == '1' ?
					new InlineKeyboardButton[][] {
						accNameButton,
						linkButton,
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"🗑 " + Localization.GetMessage("DeleteLink", user.Lang),
								"UE" + accountId) },
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

				await Bot.Instance.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
					callbackQuery.Message.MessageId,
					"*" + Localization.GetMessage("ChooseWhatUpdate", user.Lang) + "* \n\n" + callbackQuery.Message.Text,
					replyMarkup: keyboardMarkup,
					parseMode: ParseMode.Markdown,
					disableWebPagePreview: true);
			}
			else {
				await Bot.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
				string accountDataMessage = callbackQuery.Message.Text.Substring(callbackQuery.Message.Text.IndexOf('\n'));
				switch (callbackQuery.Data[1]) {
					case 'N': {
						//Account Name
						//
						//TODO:
						//add to list of waiting for update enum AccountUpdates.AccountName
						//
						await Bot.Instance.Client.EditMessageTextAsync(
							callbackQuery.From.Id,
							callbackQuery.Message.MessageId,
							string.Format(Localization.GetMessage("UpdateAccData", user.Lang),
								"*" + Localization.GetMessage("AccountName", user.Lang) + "*") + accountDataMessage,
							disableWebPagePreview: true,
							parseMode: ParseMode.Markdown);
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
}