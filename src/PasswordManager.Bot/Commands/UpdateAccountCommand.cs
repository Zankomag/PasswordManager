using AutoMapper;
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
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.Bot.Commands; 

public class UpdateAccountCommand : Abstractions.BotCommand, ICallbackQueryCommand, IActionCommand, IReplyActionCommand {
	private readonly IAccountService accountService;
	private readonly ITelegramBotUi telegramBotUi;
	private readonly IAccountUpdatingService accountUpdatingService;
	private readonly IUserService userService;
	private readonly IMapper mapper;

	public UpdateAccountCommand(IBot bot, IAccountService accountService,
		ITelegramBotUi telegramBotUi, IAccountUpdatingService accountUpdatingService,
		IUserService userService, IMapper mapper) : base(bot) {
		this.accountService = accountService;
		this.telegramBotUi = telegramBotUi;
		this.accountUpdatingService = accountUpdatingService;
		this.userService = userService;
		this.mapper = mapper;
	}

	private async Task HandleNextStage(BotUser botUser, AccountUpdatingStage nextAccountUpdatingStage,
		long? accountId = null, int? messageToEditId = null) {

		if (nextAccountUpdatingStage == AccountUpdatingStage.Release) {
			Account updatingAccount = accountUpdatingService.ReleaseAccount(botUser.Id);
			Account account = await accountService.GetAccountAsync(botUser.Id, updatingAccount.Id);
			if ((account != null) && ((accountId == null) || (accountId == account.Id))) {
				mapper.Map(updatingAccount, account);
				await accountService.UpdateAccountAsync();
				await telegramBotUi.ShowAccountAsync(botUser, account, messageToEditId,
					Localization.GetMessage("AccountUpdated", botUser.Lang));
			}
			await userService.UpdateActionAsync(botUser.Id, UserAction.Search);
			//TODO: Move next section to SendTextStage instruction method and add there this instruction and all from
			//callback query method
		} else if (nextAccountUpdatingStage == AccountUpdatingStage.EncryptPassword && accountId.HasValue) {
			await Bot.Client.SendTextMessageAsync(botUser.Id,
				"🔐 " + Localization.GetMessage("AddEncryptionKey", botUser.Lang),
				replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(
					"⏩ " + Localization.GetMessage("Skip", botUser.Lang),
					UpdateAccountCommandCode.SkipPasswordEncryption.ToStringCode(accountId.Value))));
		} else if (nextAccountUpdatingStage == AccountUpdatingStage.None) {
			accountUpdatingService.FinishUpdatingRequest(botUser.Id);
			await userService.UpdateActionAsync(botUser.Id, UserAction.Search);
			//TODO: make method in botUISrevice that sends or edits message with "Cancel" message
			await Bot.Client.SendTextMessageAsync(botUser.Id,
				Localization.GetMessage("Cancel", botUser.Lang));
		}
	}

	//TODO:
	//use emoji from new system
	async Task UpdateAccountData(BotUser botUser, Account account, UpdateAccountCommandCode updateAccountCommandCode,
		CallbackQuery callbackQuery, long accountId) {
		var backButton = InlineKeyboardButton.WithCallbackData(
			"⬅️ " + Localization.GetMessage("Back", botUser.Lang),
			UpdateAccountCommandCode.SelectUpdateType.ToStringCode(accountId));

		if (account != null) {
			(string message, InlineKeyboardMarkup replyMarkup)
				= updateAccountCommandCode switch {
					UpdateAccountCommandCode.AccountName => ("📝 " + String.Format(
							Localization.GetMessage("UpdateAccData", botUser.Lang),
							Localization.GetMessage("NewAccountName", botUser.Lang)),
						new InlineKeyboardMarkup(backButton)),
					UpdateAccountCommandCode.Link => ("🔗 " + String.Format(
							Localization.GetMessage("UpdateAccData", botUser.Lang),
							Localization.GetMessage("NewLink", botUser.Lang)),
						new InlineKeyboardMarkup(
							new InlineKeyboardButton[]{
								backButton,
								InlineKeyboardButton.WithCallbackData(
									"🗑 " + Localization.GetMessage("DeleteLink", botUser.Lang),
									UpdateAccountCommandCode.DeleteLink.ToStringCode(accountId)) })),
					UpdateAccountCommandCode.Note => ("🗒 " + String.Format(
							Localization.GetMessage("UpdateAccData", botUser.Lang),
							Localization.GetMessage("NewNote", botUser.Lang)),
						new InlineKeyboardButton[]{
							backButton,
							InlineKeyboardButton.WithCallbackData(
								"🗑 " + Localization.GetMessage("DeleteNote", botUser.Lang),
								UpdateAccountCommandCode.DeleteNote.ToStringCode(accountId)) }),
					UpdateAccountCommandCode.Login => ("📇 " + String.Format(
							Localization.GetMessage("UpdateAccData", botUser.Lang),
							Localization.GetMessage("NewLogin", botUser.Lang)),
						new InlineKeyboardMarkup(backButton)),
					UpdateAccountCommandCode.Password => ("🔑 " + String.Format(
							Localization.GetMessage("UpdateAccData", botUser.Lang),
							Localization.GetMessage("NewPassword", botUser.Lang)),
						new InlineKeyboardMarkup(
							new InlineKeyboardButton[][] {
								new InlineKeyboardButton[]{ backButton},
								telegramBotUi.GeneratePasswordKeyboard(botUser, GeneratePasswordCommandCode.Updating,
									SetUpPasswordGeneratorCommandCode.ReturnUpdating, accountId) })),
					UpdateAccountCommandCode.OutdatedTime => ("🕜 " + String.Format(
							Localization.GetMessage("UpdateAccData", botUser.Lang),
							Localization.GetMessage("NewOutdatedTime", botUser.Lang)),
						new InlineKeyboardMarkup(backButton)),
					_ => throw new InvalidOperationException()
				};
			message = await telegramBotUi.SerializeAccountAsync(botUser, account, false, message);
			await Bot.Client.EditMessageTextAsync(botUser.Id, callbackQuery.Message.MessageId,
				message, replyMarkup: replyMarkup);
			accountUpdatingService.StartUpdatingRequest(botUser.Id, account, (AccountUpdatingStage)updateAccountCommandCode);
			await userService.UpdateActionAsync(botUser.Id, UserAction.UpdateAccount);
		}
	}

	async Task DeleteAccountData(BotUser botUser, UpdateAccountCommandCode updateAccountCommandCode,
		CallbackQuery callbackQuery, long accountId) {
		AccountUpdatingStage accountUpdatingStage = updateAccountCommandCode switch {
			UpdateAccountCommandCode.DeleteLink => AccountUpdatingStage.Link,
			UpdateAccountCommandCode.DeleteNote => AccountUpdatingStage.Note,
			_ => throw new InvalidOperationException()
		};
		await UpdateAccountData(botUser, null, accountUpdatingStage, accountId, callbackQuery, false);
	}

	async Task UpdateAccountData(BotUser botUser, string property, AccountUpdatingStage accountUpdatingStage,
		long accountId, CallbackQuery callbackQuery, bool startUpdatingRequest, Account account = null) {
		try {
			if(startUpdatingRequest) {
				if (account != null) 
					accountUpdatingService.StartUpdatingRequest(botUser.Id, account, AccountUpdatingStage.OutdatedTime);
				else throw new InvalidOperationException("Account cannot be null when starting updating request");
			}
			AccountUpdatingStage nextUpdatingStage = accountUpdatingService.GetNextUpdatingStage(botUser.Id, property,
				accountUpdatingStage, accountId);
			await HandleNextStage(botUser, nextUpdatingStage, accountId, callbackQuery.Message.MessageId);
		} catch (ValidationException exception) {
			await telegramBotUi.SendValidationErrorAsync(botUser, exception);
		}
	}

	async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser botUser) {
		await Bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
		UpdateAccountCommandCode updateAccountCommandCode;
		long accountId;
		try {
			updateAccountCommandCode = (UpdateAccountCommandCode)callbackQuery.Data[1];
			string accountIdString = callbackQuery.Data[2..];
			//todo make via TryParse as in ShowAccountCommand
			accountId = Convert.ToInt64(accountIdString);
		} catch (Exception exeption) {
			//TODO: Log Exception
			throw;
		}

		Account account = await accountService.GetAccountAsync(botUser.Id, accountId);
		AccountUpdatingStage nextUpdatingStage = AccountUpdatingStage.None;

		switch (updateAccountCommandCode) {
			case UpdateAccountCommandCode.SelectUpdateType:
				if (account != null) {
					await telegramBotUi.ShowAccountUpdatingMenuAsync(botUser, account, callbackQuery.Message.MessageId,
						Localization.GetMessage("ChooseWhatUpdate", botUser.Lang));
				}
				break;
			case UpdateAccountCommandCode.Password:
			case UpdateAccountCommandCode.AccountName:
			case UpdateAccountCommandCode.Link:
			case UpdateAccountCommandCode.Note:
			case UpdateAccountCommandCode.Login:
			case UpdateAccountCommandCode.OutdatedTime:
				await UpdateAccountData(botUser, account, updateAccountCommandCode, callbackQuery, accountId);
				break;
			case UpdateAccountCommandCode.UntrackOutdatedTime:
				await UpdateAccountData(botUser, "0",
					AccountUpdatingStage.OutdatedTime, accountId, callbackQuery, true, account);
				break;
			case UpdateAccountCommandCode.UseGlobalOutdatedTime:
				await UpdateAccountData(botUser, null,
					AccountUpdatingStage.OutdatedTime, accountId, callbackQuery, true, account);
				break;
			case UpdateAccountCommandCode.RemoveEncryption:
				await UpdateAccountData(botUser, callbackQuery.Message.Text,
					AccountUpdatingStage.RemovePasswordEncryption, accountId, callbackQuery, true, account);
				break;
			case UpdateAccountCommandCode.AcceptPassword:
				await UpdateAccountData(botUser, callbackQuery.Message.Text,
					AccountUpdatingStage.Password, accountId, callbackQuery, true, account);
				break;
			case UpdateAccountCommandCode.DeleteLink:
			case UpdateAccountCommandCode.DeleteNote:
				await DeleteAccountData(botUser, updateAccountCommandCode, callbackQuery, accountId);
				break;
			case UpdateAccountCommandCode.SkipPasswordEncryption:
				nextUpdatingStage = accountUpdatingService.SkipNextUpdatingStage(botUser.Id, accountId,
					AccountUpdatingStage.EncryptPassword);
				await HandleNextStage(botUser, nextUpdatingStage, accountId, callbackQuery.Message.MessageId);
				break;
		}
	}

	async Task IActionCommand.ExecuteAsync(Message message, BotUser botUser) {
		AccountUpdatingStage nextUpdatingStage = AccountUpdatingStage.None;
		long? accountId = null;
		try {
			(accountId, nextUpdatingStage) = accountUpdatingService.GetNextUpdatingStageAndAccountId(botUser.Id, message.Text);
		} catch (ValidationException exception) {
			await telegramBotUi.SendValidationErrorAsync(botUser, exception);
		} catch (InvalidOperationException) {
			accountUpdatingService.FinishUpdatingRequest(botUser.Id);
			await userService.UpdateActionAsync(botUser.Id, UserAction.Search);
		}

		await HandleNextStage(botUser, nextUpdatingStage, accountId);
	}

	async Task IReplyActionCommand.ExecuteAsync(Message message, BotUser botUser) {
		//This command allows to bypass pressing "Accept password" button and then entering encryption key
		//by sending encryption key right in reply to suggested password message
		if (message.ReplyToMessage.ReplyMarkup != null
			&& message.ReplyToMessage.ReplyMarkup.InlineKeyboard
				.Any(x => x.Any(y => !String.IsNullOrEmpty(y.CallbackData)
					&& y.CallbackData[..2] == UpdateAccountCommandCode.AcceptPassword.ToStringCode()))) {
			AccountUpdatingStage nextUpdatingStage = AccountUpdatingStage.None;
			long? accountId = null;
			try {
				accountUpdatingService.GetNextUpdatingStageAndAccountId(
					botUser.Id, message.ReplyToMessage.Text, AccountUpdatingStage.Password);
				(accountId, nextUpdatingStage) = accountUpdatingService.GetNextUpdatingStageAndAccountId(
					botUser.Id, message.Text, AccountUpdatingStage.EncryptPassword);
			} catch (ValidationException exception) {
				await telegramBotUi.SendValidationErrorAsync(botUser, exception);
			} catch (InvalidOperationException) {
				await userService.UpdateActionAsync(botUser.Id, UserAction.Search);
			}

			await HandleNextStage(botUser, nextUpdatingStage, accountId);
		} else {
			//handle reply action command as regular action command
			await (this as IActionCommand).ExecuteAsync(message, botUser);
		}
	}

}