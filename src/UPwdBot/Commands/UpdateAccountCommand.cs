using System.Threading.Tasks;
using Telegram.Bot.Types;
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
			else {
				string accountDataMessage = callbackQuery.Message.Text.Substring(callbackQuery.Message.Text.IndexOf('\n')+2);
				switch (callbackQuery.Data[1]) {
					case 'P': {
						//Password
						PasswordManager.SetUserAction(user, Actions.Update);
						break;
					}
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
						PasswordManager.SetUserAction(user, Actions.Update);
						break;
					}
					case 'R': {
						//Link
						PasswordManager.SetUserAction(user, Actions.Update);
						break;
					}
					case 'L': {
						//Login
						PasswordManager.SetUserAction(user, Actions.Update);
						break;
					}
					case 'E': {
						//Delete Link
						List<string> accountData = accountDataMessage.Split('\n').ToList();
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
						break;
					}
				}
				
			}
		}

		public async Task ExecuteAsync(Message message, Types.User user) {

		}
	}
}