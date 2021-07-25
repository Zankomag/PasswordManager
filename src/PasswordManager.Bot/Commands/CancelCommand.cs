using System.Threading.Tasks;
using Telegram.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Commands {
	public class CancelCommand : Abstractions.BotCommand, IMessageCommand {
		private readonly IUserService userService;
		private readonly IAccountAssemblingService accountAssemblingService;
		private readonly IAccountUpdatingService accountUpdatingService;
		private readonly IPasswordDecryptionService passwordDecryptionService;
		private readonly IPasswordEncryptionService passwordEncryptionService;

		public CancelCommand(IBot bot,
			IUserService userService,
			IAccountAssemblingService accountAssemblingService,
			IAccountUpdatingService accountUpdatingService,
			IPasswordDecryptionService passwordDecryptionService,
			IPasswordEncryptionService passwordEncryptionService)
			: base(bot) {

			this.userService = userService;
			this.accountAssemblingService = accountAssemblingService;
			this.accountUpdatingService = accountUpdatingService;
			this.passwordDecryptionService = passwordDecryptionService;
			this.passwordEncryptionService = passwordEncryptionService;
		}

		//TODO:
		//instead of userService.UpdateUserAction we need to call
		//PasswordManager.Bot.Services.UserActionService.ResetUserAction(user.Id) so
		//if there is attempt to override action this service checks whether there is unfinished action
		//and clears its data
		async Task IMessageCommand.ExecuteAsync(Message message, BotUser user) {
			if (user.Action != UserAction.Search) {
				switch (user.Action) {
					case UserAction.AssembleAccount:
						accountAssemblingService.Cancel(user.Id);
						break;
					case UserAction.UpdateAccount:
						accountUpdatingService.FinishUpdatingRequest(user.Id);
						break;
					case UserAction.EnterDecryptionKey:
						passwordDecryptionService.FinishDecryptionRequest(user.Id);
						break;
					case UserAction.EncryptPassword:
						passwordEncryptionService.FinishEncryptionRequest(user.Id);
						break;
						//TODO:
						//Clear UserSettingsUpdatingservice request here
						//
						//TODO:
						//If UserAction.SetUpPasswordGeneratorLength expands to having setuppasswordgenerator service
						//finish remaining requests here
						//
						//UserAction.SetUpPasswordGeneratorLength does not need to be handeled here, 
						//as updating action to search alredy cancels action
				}

				await userService.UpdateActionAsync(user.Id, UserAction.Search);
				await Bot.Client.SendTextMessageAsync(message.From.Id,
					Localization.GetMessage("Cancel", user.Lang));
			}
			else {
				await Bot.Client.SendTextMessageAsync(message.From.Id,
					Localization.GetMessage("NoCancel", user.Lang));
			}
		}
	}
}
