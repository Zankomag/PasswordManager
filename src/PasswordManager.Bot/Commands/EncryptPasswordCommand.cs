using MultiUserLocalization;
using PasswordManager.Application.Encryption;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Commands; 

public class EncryptPasswordCommand : ShowReplyInstructionCommand, ICallbackQueryCommand, IReplyActionCommand {
	private readonly IAccountService accountService;
	private readonly IUserService userService;
	private readonly IPasswordEncryptionService passwordEncryptionService;

	public EncryptPasswordCommand(IBot bot, IAccountService accountService,
		IUserService userService, IPasswordEncryptionService passwordEncryptionService) : base(bot) {
		this.accountService = accountService;
		this.userService = userService;
		this.passwordEncryptionService = passwordEncryptionService;
	}

	async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser botUser) {
		long accountId;
		try {
			accountId = Convert.ToInt64(callbackQuery.Data[1..]);
		} catch (Exception exception) {
			//TODO: Log exception
			throw;
		}
		passwordEncryptionService.StartEncryptionRequest(botUser.Id, accountId);
		await userService.UpdateActionAsync(botUser.Id, UserAction.EncryptPassword);
		await Bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id,
			Localization.GetMessage("EncryptInstruction", botUser.Lang),
			showAlert: true);
	}

	async Task IReplyActionCommand.ExecuteAsync(Message message, BotUser botUser) {
		string callbackData = null;
		if (message.ReplyToMessage.ReplyMarkup != null
			&& message.ReplyToMessage.ReplyMarkup.InlineKeyboard
				.Any(x => x.Any(y => !String.IsNullOrEmpty(y.CallbackData)
					&& (callbackData = y.CallbackData)[0] == (char)CallbackQueryCommandCode.EncryptPassword))) {

			long passwordAccountId;
			try {
				passwordAccountId = Convert.ToInt64(callbackData[1..]);
			} catch (Exception exception) {
				//TODO: Log exception
				throw;
			}
			long? accountId = passwordEncryptionService.GetAccountId(botUser.Id);
			if (accountId != null && message.ReplyToMessage.Text != null) {
				if (accountId == passwordAccountId) {
					string encryptedPassword;
					try {
						encryptedPassword = message.ReplyToMessage.Text
							.Encrypt(message.Text);
					} catch (Exception exception) {
						//TODO; Log Exception
						throw;
					}
					await accountService.UpdatePasswordAsync(botUser.Id, accountId.Value,
						encryptedPassword, true);
					passwordEncryptionService.FinishEncryptionRequest(botUser.Id);
					return;
				}
				await ReportWrongReply(botUser);
				return;
			}
			await userService.UpdateActionAsync(botUser.Id, UserAction.Search);
			await Bot.Client.SendTextMessageAsync(message.From.Id,
				Localization.GetMessage("Cancel", botUser.Lang));
			return;
		}
		await ReportWrongReply(botUser);
	}

	async Task IActionCommand.ExecuteAsync(Message message, BotUser botUser) 
		=> await Bot.Client.SendTextMessageAsync(botUser.Id,
			Localization.GetMessage("SendKeyInReplyToPasswordMessage", botUser.Lang));

	private async Task ReportWrongReply(BotUser botUser) {
		await Bot.Client.SendTextMessageAsync(botUser.Id,
			Localization.GetMessage("NotPasswordMessageReply", botUser.Lang));
	}

}