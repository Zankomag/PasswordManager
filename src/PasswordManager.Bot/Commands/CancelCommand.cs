using System.Threading.Tasks;
using Telegram.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Types.Enums;
using User = PasswordManager.Core.Entities.User;
using UserAction = PasswordManager.Core.Entities.User.UserAction;
using PasswordManager.Bot.Commands.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class CancelCommand : IMessageCommand {
		public async Task ExecuteAsync(Message message, User user) {
			if (user.Action != UserAction.Search) {
				PasswordManagerHandler.AssemblingAccounts.Remove(message.From.Id);

				PasswordManagerHandler.SetUserAction(user.Id, UserAction.Search);
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
