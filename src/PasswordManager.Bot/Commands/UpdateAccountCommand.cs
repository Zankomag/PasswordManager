using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using MultiUserLocalization;
using PasswordManager.Bot.Types;
using System.Linq;
using System.Collections.Generic;
using PasswordManager.Bot.Types.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Core.Entities;
using User = PasswordManager.Core.Entities.User;
using UserAction = PasswordManager.Core.Entities.User.UserAction;
using PasswordManager.Bot.Commands.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class UpdateAccountCommand : ICallBackQueryCommand, IMessageCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, User user) {
			string accountId = callbackQuery.Data.Substring(2);
			if (callbackQuery.Data[1] == '0') {
				await PasswordManagerHandler.UpdateAccountAsync(
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

				PasswordManagerHandler.DeleteAccountLink(user, accountId);
				await PasswordManagerHandler.UpdateAccountAsync(
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
				PasswordManagerHandler.SetUserAction(user, UserAction.Update);
				AccountDataType accountDataType = (AccountDataType)(byte)callbackQuery.Data[1];
				InlineKeyboardMarkup inlineKeyboardMarkup = accountDataType == AccountDataType.Password ? 
					PasswordManagerHandler.GeneratePasswordButtonMarkup(user.Lang) : 
					null;
				Message sentMessage = await RequestUpdateData(callbackQuery.From.Id, accountDataType.ToString(), user.Lang, inlineKeyboardMarkup);
				PasswordManagerHandler.UpdatingAccounts[callbackQuery.From.Id] = new AccountUpdate(
					accountId,
					callbackQuery.Message.MessageId,
					sentMessage.MessageId,
					accountDataType);
			}
		}

		public async Task ExecuteAsync(Message message, User user) {
			if (PasswordManagerHandler.UpdatingAccounts.ContainsKey(user.Id)) {
				AccountUpdate accountUpdate = PasswordManagerHandler.UpdatingAccounts[user.Id];
				if (!await PasswordManagerHandler.IsLengthExceededAsync(message.Text.Length, accountUpdate.AccountDataType.ToMaxAccountDataLength(), user.Id, user.Lang)) {
					await PasswordManagerHandler.UpdateAccountDataAsync(message.Text, accountUpdate.AccountToUpdateId, user.Id, user.Lang);
				}
			} else {
				PasswordManagerHandler.SetUserAction(user, UserAction.Search);
			}
		}

		private async Task<Message> RequestUpdateData(ChatId chatId, string dataKey, string langCode, InlineKeyboardMarkup inlineKeyboardMarkup = null) {
			return await Bot.Instance.Client.SendTextMessageAsync(
				chatId,
				string.Format(Localization.GetMessage("UpdateAccData", langCode), Localization.GetMessage(dataKey, langCode)),
				replyMarkup: inlineKeyboardMarkup);
		}

	}
}