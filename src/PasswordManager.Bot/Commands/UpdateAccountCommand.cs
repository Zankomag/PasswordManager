using AutoMapper;
using MultiUserLocalization;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Bot.Services.Enums;
using PasswordManager.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.Bot.Commands {
	public class UpdateAccountCommand : Abstractions.BotCommand, ICallbackQueryCommand, IActionCommand, IReplyActionCommand {
		private readonly IAccountService accountService;
		private readonly IBotUIService botUIService;
		private readonly IAccountUpdatingService accountUpdatingService;
		private readonly IUserService userService;
		private readonly IMapper mapper;

		public UpdateAccountCommand(IBot bot, IAccountService accountService,
			IBotUIService botUIService, IAccountUpdatingService accountUpdatingService,
			IUserService userService, IMapper mapper) : base(bot) {
			this.accountService = accountService;
			this.botUIService = botUIService;
			this.accountUpdatingService = accountUpdatingService;
			this.userService = userService;
			this.mapper = mapper;
		}

		private async Task HandleNextStage(BotUser user, AccountUpdatingStage nextAccountUpdatingStage,
			long? accountId = null, int? messageToEditId = null) {

			if (nextAccountUpdatingStage == AccountUpdatingStage.Release) {
				Account updatingAccount = accountUpdatingService.ReleaseAccount(user.Id);
				Account account = await accountService.GetFullAsync(user.Id, updatingAccount.Id);
				if ((account != null) && ((accountId == null) || (accountId == account.Id))) {
					mapper.Map(updatingAccount, account);
					await accountService.UpdateAccountAsync();
					await botUIService.ShowAccount(user, account, messageToEditId,
						Localization.GetMessage("AccountUpdated", user.Lang));
				}
				await userService.UpdateActionAsync(user.Id, UserAction.Search);
				//TODO: Move next section to SendTextStage instruction method and add there this instruction and all from
				//callback query method
			} else if (nextAccountUpdatingStage == AccountUpdatingStage.EncryptPassword && accountId.HasValue) {
				await bot.Client.SendTextMessageAsync(user.Id,
					"🔐 " + Localization.GetMessage("AddEncryptionKey", user.Lang),
					replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(
						"⏩ " + Localization.GetMessage("Skip", user.Lang),
						UpdateAccountCommandCode.SkipPasswordEncryption.ToStringCode(accountId.Value))));
			} else if (nextAccountUpdatingStage == AccountUpdatingStage.None) {
				accountUpdatingService.FinishUpdatingRequest(user.Id);
				await userService.UpdateActionAsync(user.Id, UserAction.Search);
				//TODO: make method in botUISrevice that sends or edits message with "Cancel" message
				await bot.Client.SendTextMessageAsync(user.Id,
					Localization.GetMessage("Cancel", user.Lang));
			}
		}

		//TODO:
		//use emoji from new system
		async Task UpdateAccountData(BotUser user, Account account, UpdateAccountCommandCode updateAccountCommandCode,
			CallbackQuery callbackQuery, long accountId) {
			var backButton = InlineKeyboardButton.WithCallbackData(
				"⬅️ " + Localization.GetMessage("Back", user.Lang),
				UpdateAccountCommandCode.SelectUpdateType.ToStringCode(accountId));

			if (account != null) {
				(string message, InlineKeyboardMarkup replyMarkup)
					= updateAccountCommandCode switch {
						UpdateAccountCommandCode.AccountName => ("📝 " + string.Format(
								Localization.GetMessage("UpdateAccData", user.Lang),
								Localization.GetMessage("NewAccountName", user.Lang)),
							new InlineKeyboardMarkup(backButton)),
						UpdateAccountCommandCode.Link => ("🔗 " + string.Format(
								Localization.GetMessage("UpdateAccData", user.Lang),
								Localization.GetMessage("NewLink", user.Lang)),
							new InlineKeyboardMarkup(
								new InlineKeyboardButton[]{
									backButton,
									InlineKeyboardButton.WithCallbackData(
										"🗑 " + Localization.GetMessage("DeleteLink", user.Lang),
										UpdateAccountCommandCode.DeleteLink.ToStringCode(accountId)) })),
						UpdateAccountCommandCode.Note => ("🗒 " + string.Format(
								Localization.GetMessage("UpdateAccData", user.Lang),
								Localization.GetMessage("NewNote", user.Lang)),
							new InlineKeyboardButton[]{
									backButton,
									InlineKeyboardButton.WithCallbackData(
										"🗑 " + Localization.GetMessage("DeleteNote", user.Lang),
										UpdateAccountCommandCode.DeleteNote.ToStringCode(accountId)) }),
						UpdateAccountCommandCode.Login => ("📇 " + string.Format(
								Localization.GetMessage("UpdateAccData", user.Lang),
								Localization.GetMessage("NewLogin", user.Lang)),
							new InlineKeyboardMarkup(backButton)),
						UpdateAccountCommandCode.Password => ("🔑 " + string.Format(
								Localization.GetMessage("UpdateAccData", user.Lang),
								Localization.GetMessage("NewPassword", user.Lang)),
							new InlineKeyboardMarkup(
								new InlineKeyboardButton[][] {
									new InlineKeyboardButton[]{ backButton},
									botUIService.GeneratePasswordKeyboard(user, GeneratePasswordCommandCode.Updating,
										SetUpPasswordGeneratorCommandCode.ReturnUpdating, accountId) })),
						UpdateAccountCommandCode.OutdatedTime => ("🕜 " + string.Format(
								Localization.GetMessage("UpdateAccData", user.Lang),
								Localization.GetMessage("NewOutdatedTime", user.Lang)),
							new InlineKeyboardMarkup(backButton)),
						_ => throw new InvalidOperationException()
					};
				message = await botUIService.SerializeAccount(user, account, false, message);
				await bot.Client.EditMessageTextAsync(user.Id, callbackQuery.Message.MessageId,
					message, replyMarkup: replyMarkup);
				accountUpdatingService.StartUpdatingRequest(user.Id, account, (AccountUpdatingStage)updateAccountCommandCode);
				await userService.UpdateActionAsync(user.Id, UserAction.UpdateAccount);
			}
		}

		async Task DeleteAccountData(BotUser user, UpdateAccountCommandCode updateAccountCommandCode,
			CallbackQuery callbackQuery, long accountId) {
			AccountUpdatingStage accountUpdatingStage = updateAccountCommandCode switch {
				UpdateAccountCommandCode.DeleteLink => AccountUpdatingStage.Link,
				UpdateAccountCommandCode.DeleteNote => AccountUpdatingStage.Note,
				_ => throw new InvalidOperationException()
			};
			await UpdateAccountData(user, null, accountUpdatingStage, accountId, callbackQuery, false);
		}

		async Task UpdateAccountData(BotUser user, string property, AccountUpdatingStage accountUpdatingStage,
			long accountId, CallbackQuery callbackQuery, bool startUpdatingRequest, Account account = null) {
			try {
				if(startUpdatingRequest) {
					if (account != null) 
						accountUpdatingService.StartUpdatingRequest(user.Id, account, AccountUpdatingStage.OutdatedTime);
					else throw new InvalidOperationException("Account cannot be null when starting updating request");
				}
				AccountUpdatingStage nextUpdatingStage = accountUpdatingService.GetNextUpdatingStage(user.Id, property,
					accountUpdatingStage, accountId);
				await HandleNextStage(user, nextUpdatingStage, accountId, callbackQuery.Message.MessageId);
			} catch (ValidationException exception) {
				await botUIService.SendValidationError(user, exception);
			}
		}

		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			await bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			UpdateAccountCommandCode updateAccountCommandCode;
			string accoundIdString;
			long accountId;
			try {
				updateAccountCommandCode = (UpdateAccountCommandCode)callbackQuery.Data[1];
				accoundIdString = callbackQuery.Data[2..];
				accountId = Convert.ToInt64(accoundIdString);
			} catch (Exception exeption) {
				//TODO: Log Exception
				throw;
			}

			Account account = await accountService.GetFullAsync(user.Id, accountId);
			AccountUpdatingStage nextUpdatingStage = AccountUpdatingStage.None;

			switch (updateAccountCommandCode) {
				case UpdateAccountCommandCode.SelectUpdateType:
					if (account != null) {
						await botUIService.ShowAccountUpdatingMenuAsync(user, account, callbackQuery.Message.MessageId,
							Localization.GetMessage("ChooseWhatUpdate", user.Lang));
					}
					break;
				case UpdateAccountCommandCode.Password:
				case UpdateAccountCommandCode.AccountName:
				case UpdateAccountCommandCode.Link:
				case UpdateAccountCommandCode.Note:
				case UpdateAccountCommandCode.Login:
				case UpdateAccountCommandCode.OutdatedTime:
					await UpdateAccountData(user, account, updateAccountCommandCode, callbackQuery, accountId);
					break;
				case UpdateAccountCommandCode.UntrackOutdatedTime:
					await UpdateAccountData(user, "0",
						AccountUpdatingStage.OutdatedTime, accountId, callbackQuery, true, account);
					break;
				case UpdateAccountCommandCode.UseGlobalOutdatedTime:
					await UpdateAccountData(user, null,
						AccountUpdatingStage.OutdatedTime, accountId, callbackQuery, true, account);
					break;
				case UpdateAccountCommandCode.RemoveEncryption:
					await UpdateAccountData(user, callbackQuery.Message.Text,
						AccountUpdatingStage.RemovePasswordEncryption, accountId, callbackQuery, true, account);
					break;
				case UpdateAccountCommandCode.AcceptPassword:
					await UpdateAccountData(user, callbackQuery.Message.Text,
						AccountUpdatingStage.Password, accountId, callbackQuery, true, account);
					break;
				case UpdateAccountCommandCode.DeleteLink:
				case UpdateAccountCommandCode.DeleteNote:
					await DeleteAccountData(user, updateAccountCommandCode, callbackQuery, accountId);
					break;
				case UpdateAccountCommandCode.SkipPasswordEncryption:
					nextUpdatingStage = accountUpdatingService.SkipNextUpdatingStage(user.Id, accountId,
						AccountUpdatingStage.EncryptPassword);
					await HandleNextStage(user, nextUpdatingStage, accountId, callbackQuery.Message.MessageId);
					break;
			}
		}

		async Task IActionCommand.ExecuteAsync(Message message, BotUser user) {
			AccountUpdatingStage nextUpdatingStage = AccountUpdatingStage.None;
			long? accountId = null;
			try {
				(accountId, nextUpdatingStage) = accountUpdatingService.GetNextUpdatingStageAndAccountId(user.Id, message.Text);
			} catch (ValidationException exception) {
				await botUIService.SendValidationError(user, exception);
			} catch (InvalidOperationException) {
				accountUpdatingService.FinishUpdatingRequest(user.Id);
				await userService.UpdateActionAsync(user.Id, UserAction.Search);
			}

			await HandleNextStage(user, nextUpdatingStage, accountId);
		}

		async Task IReplyActionCommand.ExecuteAsync(Message message, BotUser user) {
			//This command allows to bypass pressing "Accept password" button and then entering encryption key
			//by sending encryption key right in reply to suggested password message
			if (message.ReplyToMessage.ReplyMarkup != null
				&& message.ReplyToMessage.ReplyMarkup.InlineKeyboard
					.Any(x => x.Any(y => !string.IsNullOrEmpty(y.CallbackData)
						&& y.CallbackData[..2] == UpdateAccountCommandCode.AcceptPassword.ToStringCode()))) {
				AccountUpdatingStage nextUpdatingStage = AccountUpdatingStage.None;
				long? accountId = null;
				try {
					accountUpdatingService.GetNextUpdatingStageAndAccountId(
						user.Id, message.ReplyToMessage.Text, AccountUpdatingStage.Password);
					(accountId, nextUpdatingStage) = accountUpdatingService.GetNextUpdatingStageAndAccountId(
						user.Id, message.Text, AccountUpdatingStage.EncryptPassword);
				} catch (ValidationException exception) {
					await botUIService.SendValidationError(user, exception);
				} catch (InvalidOperationException) {
					await userService.UpdateActionAsync(user.Id, UserAction.Search);
				}

				await HandleNextStage(user, nextUpdatingStage, accountId);
			} else {
				//handle reply action command as regular action command
				await (this as IActionCommand).ExecuteAsync(message, user);
			}
		}

	}
}