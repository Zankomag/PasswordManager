using System.Threading.Tasks;
using Telegram.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class CancelCommand : IMessageCommand {
		public async Task ExecuteAsync(Message message, BotUser user) {
			if (user.Action != UserAction.Search) {
				PasswordManagerService.AssemblingAccounts.Remove(message.From.Id);

				PasswordManagerService.SetUserAction(user.Id, UserAction.Search);
				await BotService.Instance.Client.SendTextMessageAsync(message.From.Id,
					Localization.GetMessage("Cancel", user.Lang));
			}
			else {
				await BotService.Instance.Client.SendTextMessageAsync(message.From.Id,
					Localization.GetMessage("NoCancel", user.Lang));
			}
		}
	}
}
