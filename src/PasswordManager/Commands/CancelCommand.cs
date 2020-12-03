using System.Threading.Tasks;
using Telegram.Bot.Types;
using Uten.Localization.MultiUser;
using PasswordManager.Types.Enums;

namespace PasswordManager.Commands {
	public class CancelCommand : IMessageCommand {
		public async Task ExecuteAsync(Message message, Types.User user) {
			if (user.ActionType != UserAction.Search) {
				PasswordManager.AssemblingAccounts.Remove(message.From.Id);

				PasswordManager.SetUserAction(user.Id, UserAction.Search);
				await Bot.Instance.Client.SendTextMessageAsync(message.From.Id,
					Localization.GetMessage("Cancel", user.Lang));
			}
			else {
				await Bot.Instance.Client.SendTextMessageAsync(message.From.Id,
					Localization.GetMessage("NoCancel", user.Lang));
			}
		}
	}
}
