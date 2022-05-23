using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using PasswordManager.Bot.Services.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using System.ComponentModel.DataAnnotations;
using PasswordManager.Bot.Commands.Enums;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace PasswordManager.Bot.Commands; 

public class AddAccountCommand : Abstractions.BotCommand, IMessageCommand, IReplyActionCommand, ICallbackQueryCommand {
	private readonly IAccountService accountService;
	private readonly IUserService userService;
	private readonly IAccountAssemblingService accountAssemblingService;
	private readonly IBotUi botUi;

	public AddAccountCommand(IBot bot, IAccountService accountService,
		IUserService userService, IAccountAssemblingService accountAssemblingService,
		IBotUi botUi) : base(bot)
		=> (this.accountService, this.userService, this.accountAssemblingService, this.botUi)
			= (	 accountService,	  userService,		accountAssemblingService,	   botUi);

	async Task IMessageCommand.ExecuteAsync(Message message, BotUser botUser) {
		AccountAssemblingStage nextAccountAssemblingStage = AccountAssemblingStage.None;
		try {
			nextAccountAssemblingStage = accountAssemblingService
				.Create(botUser.Id, message.Text.GetCommandArgsByNewLine());
		} catch(ValidationException exception) {
			await botUi.SendValidationErrorAsync(botUser, exception);
		} catch (ArgumentException exception) {
			//TODO: Log exception
			throw;
		}

		await HandleNextStage(botUser, nextAccountAssemblingStage);
	}

	private async Task HandleNextStage(BotUser botUser, AccountAssemblingStage nextAccountAssemblingStage) {
		if (nextAccountAssemblingStage == AccountAssemblingStage.Release) {
			var account = accountAssemblingService.Release(botUser.Id);
			if (await accountService.AddAccountAsync(account)) {
				//TODO: use emoji by key
				await botUi.ShowAccountAsync(botUser, account,
					extraMessage: "✅ " + String.Format(Localization.GetMessage("AccountAdded", botUser.Lang),
						account.AccountName));
			}
			await userService.UpdateActionAsync(botUser.Id, UserAction.Search);
		} else {
			if (botUser.Action != UserAction.AssembleAccount)
				await userService.UpdateActionAsync(botUser.Id, UserAction.AssembleAccount);
			await SendNextStageInstruction(botUser, nextAccountAssemblingStage);
		}
	}

	//TODO:
	//Use emoji from new emoji system
	//and use new localization sysrtem where there are methods with parameters
	//and get buttons from UI bot service
	private async Task SendNextStageInstruction(BotUser botUser, AccountAssemblingStage nextAccountAssemblingStage, int? messageToEditId = null) {
		(string message, InlineKeyboardMarkup replyMarkup) = nextAccountAssemblingStage switch {
			AccountAssemblingStage.AddAccountName
				=> ("📝 " + Localization.GetMessage("AddAccount", botUser.Lang), null),
			AccountAssemblingStage.AddLink
				=> ((Func<(string, InlineKeyboardMarkup)>)(() => {
					string autoLink = accountAssemblingService.GetAccountName(botUser.Id).AutoDomain();
					return ("🔗 " + Localization.GetMessage("AddLink", botUser.Lang),
						new InlineKeyboardMarkup(
							new InlineKeyboardButton[][] {
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										"🔗 " + autoLink,
										AddAccountCommandCode.AutoLink.ToStringCode() + autoLink)},
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										"⏩ " + Localization.GetMessage("Skip",botUser.Lang),
										AddAccountCommandCode.SkipLink.ToStringCode())}})); }))(),
			AccountAssemblingStage.AddNote
				=> ("🗒 " + Localization.GetMessage("AddNote", botUser.Lang),
					new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(
						"⏩ " + Localization.GetMessage("Skip", botUser.Lang),
						AddAccountCommandCode.SkipNote.ToStringCode()))),
			AccountAssemblingStage.AddLogin
				=> ("📇 " + Localization.GetMessage("AddLogin", botUser.Lang), null),
			AccountAssemblingStage.AddPassword
				=> ("🔑 " + Localization.GetMessage("AddPassword", botUser.Lang),
					botUi.GeneratePasswordKeyboard(botUser,
						GeneratePasswordCommandCode.Assembling,
						SetUpPasswordGeneratorCommandCode.ReturnAssembling)),
			AccountAssemblingStage.AddEncryptionKey
				=> ("🔐 " + Localization.GetMessage("AddEncryptionKey", botUser.Lang),
					new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(
						"⏩ " + Localization.GetMessage("Skip", botUser.Lang),
						AddAccountCommandCode.SkipEncryptionKey.ToStringCode()))),
			_ => throw new ArgumentException("Unexcpected AccountAssemblingStage")
		};

		if (messageToEditId == null) {
			await Bot.Client
				.SendTextMessageAsync(botUser.Id, message,
					replyMarkup: replyMarkup,
					parseMode: ParseMode.Markdown);
		} else {
			await Bot.Client.EditMessageTextAsync(
				botUser.Id, messageToEditId.Value, message,
				replyMarkup: replyMarkup,
				parseMode: ParseMode.Markdown);
		}
	}

	//todo: delete this comment below
	//Moved from password manager
	private async Task ReportExceededLength(BotUser botUser, int maxAccountDataLength, string accountDataType) {
		await Bot.Client.SendTextMessageAsync(botUser.Id,
			String.Format(Localization.GetMessage("MaxLength", botUser.Lang),
				Localization.GetMessage(accountDataType, botUser.Lang),
				maxAccountDataLength));
	}

	//TODO: Refactor
	async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser botUser) {
		AddAccountCommandCode addAccountCommandCode;
		try {
			addAccountCommandCode = (AddAccountCommandCode)callbackQuery.Data[1];
		} catch (Exception exeption) {
			//TODO: Log Exception
			throw;
		}

		(string propertyToAssemble, AccountAssemblingStage assemblingStage) = addAccountCommandCode switch {
			AddAccountCommandCode.SkipLink 
				=> (null, (AccountAssemblingStage)AccountAssemblingStageSkip.AddLink),
			AddAccountCommandCode.SkipNote 
				=> (null, (AccountAssemblingStage)AccountAssemblingStageSkip.AddNote),
			AddAccountCommandCode.SkipEncryptionKey 
				=> (null, (AccountAssemblingStage)AccountAssemblingStageSkip.AddEncryptionKey),
			AddAccountCommandCode.AutoLink 
				=> (callbackQuery.Data[2..], AccountAssemblingStage.AddLink),
			AddAccountCommandCode.AcceptPassword 
				=> (callbackQuery.Message.Text, AccountAssemblingStage.AddPassword),
			_ => throw new NotImplementedException()
		};

		try {
			AccountAssemblingStage nextStage = propertyToAssemble == null
				? accountAssemblingService.SkipStage(botUser.Id,
					(AccountAssemblingStageSkip)assemblingStage)
				: accountAssemblingService.Assemble(botUser.Id,
					propertyToAssemble, assemblingStage);
			await SendNextStageInstruction(botUser, nextStage,
				callbackQuery.Message.MessageId);
		} catch (ValidationException exception) {
			await botUi.SendValidationErrorAsync(botUser, exception);
		} catch (InvalidOperationException) {
			await ReportAbsenceOfNewAccount(botUser, callbackQuery.Id);
		} catch (Exception exception) {
			//TODO: Log Exception
			throw;
		}

			
	}

	private async Task ReportAbsenceOfNewAccount(BotUser botUser, string callbackQueryId)
		=> await Bot.Client.AnswerCallbackQueryAsync(callbackQueryId,
			text: Localization.GetMessage("CantWithoutNewAcc", botUser.Lang), showAlert: true);
		

	async Task IActionCommand.ExecuteAsync(Message message, BotUser botUser) {
		AccountAssemblingStage nextAssemblingStage = AccountAssemblingStage.None;
		try {
			nextAssemblingStage = accountAssemblingService.Assemble(botUser.Id, message.Text);
		} catch (ValidationException exception) {
			await botUi.SendValidationErrorAsync(botUser, exception);
		} catch (InvalidOperationException) {
			await userService.UpdateActionAsync(botUser.Id, UserAction.Search);
		}

		await HandleNextStage(botUser, nextAssemblingStage);
	}

	async Task IReplyActionCommand.ExecuteAsync(Message message, BotUser botUser) {
		//This command allows to bypass pressing "Accept password" button and then entering encryption key
		//by sending encryption key right in reply to suggested password message
		if (message.ReplyToMessage.ReplyMarkup != null
			&& message.ReplyToMessage.ReplyMarkup.InlineKeyboard
				.Any(x => x.Any(y => !String.IsNullOrEmpty(y.CallbackData)
					&& y.CallbackData == AddAccountCommandCode.AcceptPassword.ToStringCode()))) {
			AccountAssemblingStage nextAssemblingStage = AccountAssemblingStage.None;
			try {
				accountAssemblingService.Assemble(botUser.Id, message.ReplyToMessage.Text,
					AccountAssemblingStage.AddPassword);
				nextAssemblingStage = accountAssemblingService
					.Assemble(botUser.Id, message.Text, AccountAssemblingStage.AddEncryptionKey);
			} catch (ValidationException exception) {
				await botUi.SendValidationErrorAsync(botUser, exception);
			} catch (InvalidOperationException) {
				await userService.UpdateActionAsync(botUser.Id, UserAction.Search);
			}

			await HandleNextStage(botUser, nextAssemblingStage);
		} else {
			//handle reply action command as regular action command
			await (this as IActionCommand).ExecuteAsync(message, botUser);
		}
	}
}