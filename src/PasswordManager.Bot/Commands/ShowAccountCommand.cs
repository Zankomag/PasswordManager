using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using User = PasswordManager.Core.Entities.User;

namespace PasswordManager.Bot.Commands {
	public class ShowAccountCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, User user) {
			await Bot.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			string accountId = callbackQuery.Data.Substring(1);

			await PasswordManagerHandler.ShowAccountById(
				callbackQuery.From.Id,
				accountId,
				user.Lang,
				callbackQuery.Message.MessageId);
		}

	}
}
