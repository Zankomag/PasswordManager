using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Core.Entities;
using PasswordManager.Telegram.Commands.Abstractions;
using PasswordManager.Telegram.Models;
using PasswordManager.Telegram.Services.Abstractions;
using Telegram.Bot;

namespace PasswordManager.Telegram.Commands; 

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
	//PasswordManager.Telegram.Services.UserActionService.ResetUserAction(user.Id) so
	//if there is attempt to override action this service checks whether there is unfinished action
	//and clears its data
	async Task IMessageCommand.ExecuteAsync(Message message, BotUser botUser) {
		if (botUser.Action != UserAction.Search) {
			switch (botUser.Action) {
				case UserAction.AssembleAccount:
					accountAssemblingService.Cancel(botUser.Id);
					break;
				case UserAction.UpdateAccount:
					accountUpdatingService.FinishUpdatingRequest(botUser.Id);
					break;
				case UserAction.EnterDecryptionKey:
					passwordDecryptionService.FinishDecryptionRequest(botUser.Id);
					break;
				case UserAction.EncryptPassword:
					passwordEncryptionService.FinishEncryptionRequest(botUser.Id);
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

			await userService.UpdateActionAsync(botUser.Id, UserAction.Search);
			await Bot.Client.SendTextMessageAsync(message.From.Id,
				Localization.GetMessage("Cancel", botUser.Lang));
		}
		else {
			await Bot.Client.SendTextMessageAsync(message.From.Id,
				Localization.GetMessage("NoCancel", botUser.Lang));
		}
	}
}