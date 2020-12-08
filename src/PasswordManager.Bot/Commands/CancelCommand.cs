using System.Threading.Tasks;
using Telegram.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Application.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class CancelCommand : Abstractions.BotCommand, IMessageCommand {
		private readonly IUserService userService;

		public CancelCommand(IBotService botService, IUserService userService) : base(botService) {
			this.userService = userService;
		}

		public async Task ExecuteAsync(Message message, BotUser user) {
			if (user.Action != UserAction.Search) {
				.AssemblingAccounts.Remove(message.From.Id);

				.SetUserAction(user.Id, UserAction.Search);
				await botService.Client.SendTextMessageAsync(message.From.Id,
					Localization.GetMessage("Cancel", user.Lang));
			}
			else {
				await botService.Client.SendTextMessageAsync(message.From.Id,
					Localization.GetMessage("NoCancel", user.Lang));
			}
		}
	}
}
