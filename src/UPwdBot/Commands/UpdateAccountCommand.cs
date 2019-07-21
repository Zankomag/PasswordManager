using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Uten.Localization.MultiUser;
using UPwdBot.Types;
using System.Linq;
using System.Collections.Generic;
using UPwdBot.Types.Enums;
using UPwdBot.Extensions;

namespace UPwdBot.Commands {
	public class UpdateAccountCommand : ICallBackQueryCommand, IMessageCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			string accountId = callbackQuery.Data.Substring(2);
			if (callbackQuery.Data[1] == '0') {
				await PasswordManager.UpdateAccountAsync(
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

				PasswordManager.DeleteAccountLink(user, accountId);
				await PasswordManager.UpdateAccountAsync(
					callbackQuery.Message.Chat.Id,
					callbackQuery.Message.MessageId,
					accountId,
					Localization.GetMessage("LinkDeleted", user.Lang),
					user.Lang,
					false,
					string.Join('\n', accountData));
			}
			else {
				await Bot.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
				PasswordManager.SetUserAction(user, UserAction.Update);
				AccountDataType accountDataType = (AccountDataType)(byte)callbackQuery.Data[1];
				InlineKeyboardMarkup inlineKeyboardMarkup = accountDataType == AccountDataType.Password ? 
					new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("Generate", user.Lang),
						"G")) : 
					null;
				Message sentMessage = await RequestUpdateData(callbackQuery.From.Id, accountDataType.ToString(), user.Lang, inlineKeyboardMarkup);
				PasswordManager.UpdatingAccounts[callbackQuery.From.Id] = new AccountUpdate(
					accountId,
					callbackQuery.Message.MessageId,
					sentMessage.MessageId,
					accountDataType);
			}
		}

		public async Task ExecuteAsync(Message message, Types.User user) {
			if (PasswordManager.UpdatingAccounts.ContainsKey(user.Id)) {
				AccountUpdate accountUpdate = PasswordManager.UpdatingAccounts[user.Id];
				if (!await PasswordManager.IsLengthExceededAsync(message.Text.Length, accountUpdate.AccountDataType.ToMaxAccountDataLength(), user.Id, user.Lang)) {
					await PasswordManager.UpdateAccountDataAsync(message.Text, accountUpdate.AccountToUpdateId, user.Id, user.Lang);
				}
			} else {
				PasswordManager.SetUserAction(user, UserAction.Search);
			}
		}

		private async Task<Message> RequestUpdateData(ChatId chatId, string dataKey, string langCode, InlineKeyboardMarkup inlineKeyboardMarkup = null) {
			return await Bot.Instance.Client.SendTextMessageAsync(
				chatId,
				string.Format(Localization.GetMessage("UpdateAccData", langCode), "*" + Localization.GetMessage(dataKey, langCode) + "*"),
				parseMode: ParseMode.Markdown,
				replyMarkup: inlineKeyboardMarkup);
		}

	}
}