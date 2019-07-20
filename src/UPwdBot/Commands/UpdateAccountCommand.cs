using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Uten.Localization.MultiUser;
using UPwdBot.Types;
using System.Linq;
using System.Collections.Generic;

namespace UPwdBot.Commands {
	public class UpdateAccountCommand : ICallBackQueryCommand, IMessageCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			string accountId = callbackQuery.Data.Substring(3);
			if (callbackQuery.Data[1] == '0') {
				await PasswordManager.UpdateAccountAsync(
					callbackQuery.Message.Chat.Id,
					callbackQuery.Message.MessageId,
					accountId,
					Localization.GetMessage("ChooseWhatUpdate", user.Lang),
					user.Lang,
					callbackQuery.Data[2],
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
					'0',
					string.Join('\n', accountData));
			}
			else {
				//
				//TODO:
				//
				//DELETE MESSAGE AFTER NEW DATA HAS ACCEPTED 
				//
				//await BotHandler.TryDeleteMessageAsync(callbackQuery.From.Id, callbackQuery.Message.MessageId);
				PasswordManager.SetUserAction(user, Actions.Update);
				AccountDataTypes dataType = (AccountDataTypes)(byte)callbackQuery.Data[1];
				InlineKeyboardMarkup inlineKeyboardMarkup = dataType == AccountDataTypes.Password ? 
					new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("Generate", user.Lang),
						'G' + accountId)) : 
					null;
				PasswordManager.UpdatingAccounts[callbackQuery.From.Id] = dataType;
				await RequestUpdateData(callbackQuery.From.Id, dataType.ToString(), user.Lang, inlineKeyboardMarkup);
			}
		}

		public async Task ExecuteAsync(Message message, Types.User user) {

		}

		private async Task RequestUpdateData(ChatId chatId, string dataKey, string langCode, InlineKeyboardMarkup inlineKeyboardMarkup = null) {
			await Bot.Instance.Client.SendTextMessageAsync(
				chatId,
				string.Format(Localization.GetMessage("UpdateAccData", langCode), "*" + Localization.GetMessage(dataKey, langCode) + "*"),
				parseMode: ParseMode.Markdown,
				replyMarkup: inlineKeyboardMarkup);
		}

	}
}