using MultiUserLocalization;
using PasswordManager.Application.Encryption;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Core.Entities;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Commands {
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

		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			long accountId;
			try {
				accountId = Convert.ToInt64(callbackQuery.Data[1..]);
			} catch (Exception exception) {
				//TODO: Log exception
				throw;
			}
			passwordEncryptionService.StartEncryptionRequest(user.Id, accountId);
			await userService.UpdateActionAsync(user.Id, UserAction.EncryptPassword);
			await bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id,
				Localization.GetMessage("EncryptInstruction", user.Lang),
				showAlert: true);
		}

		async Task IReplyActionCommand.ExecuteAsync(Message message, BotUser user) {
			if (message.ReplyToMessage.ReplyMarkup != null) {
				foreach (var buttonRow in message.ReplyToMessage.ReplyMarkup.InlineKeyboard) {
					foreach (var button in buttonRow) {
						if (!string.IsNullOrEmpty(button.CallbackData)
							&& button.CallbackData[0] == (char)CallbackQueryCommandCode.EncryptPassword) {

							long passwordAccountId;
							try {
								passwordAccountId = Convert.ToInt64(button.CallbackData[1..]);
							} catch (Exception exception) {
								//TODO: Log exception
								throw;
							}
							long? accountId = passwordEncryptionService.GetAccountId(user.Id);
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
									await accountService.UpdatePasswordAsync(user.Id, accountId.Value,
										encryptedPassword, true);
									return;
								}
								await ReportWrongReply(user);
								return;
							}
						}
						await userService.UpdateActionAsync(user.Id, UserAction.Search);
						await bot.Client.SendTextMessageAsync(message.From.Id,
							Localization.GetMessage("Cancel", user.Lang));
						return;
					}
				}
			}
			await ReportWrongReply(user);
		}

		async Task IActionCommand.ExecuteAsync(Message message, BotUser user) 
			=> await bot.Client.SendTextMessageAsync(user.Id,
				Localization.GetMessage("SendKeyInReplyToPasswordMessage", user.Lang));

		private async Task ReportWrongReply(BotUser user) {
			await bot.Client.SendTextMessageAsync(user.Id,
						Localization.GetMessage("NotPasswordMessageReply", user.Lang));
		}

	}
}
