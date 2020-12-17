using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MultiUserLocalization;
using PasswordManager.Bot.Services.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using System.ComponentModel.DataAnnotations;
using PasswordManager.Bot.Commands.Enums;
using Telegram.Bot.Types.Enums;

namespace PasswordManager.Bot.Commands {
	public class AddAccountCommand : Abstractions.BotCommand, IMessageCommand, IReplyActionCommand, ICallbackQueryCommand {
		private readonly IAccountService accountService;
		private readonly IUserService userService;
		private readonly IAccountAssemblingService accountAssemblingService;
		private readonly IBotUIService botUIService;

		public AddAccountCommand(IBot bot, IAccountService accountService,
			IUserService userService, IAccountAssemblingService accountAssemblingService,
			IBotUIService botUIService) : base(bot)
			=> (this.accountService, this.userService, this.accountAssemblingService, this.botUIService)
				= (	 accountService,	  userService,		accountAssemblingService,	   botUIService);

		async Task IMessageCommand.ExecuteAsync(Message message, BotUser user) {
			AccountAssemblingStage nextAccountAssemblingStage = AccountAssemblingStage.None;
			try {
				nextAccountAssemblingStage = accountAssemblingService
					.Create(user.Id, message.Text.GetCommandArgsByNewLine());
			} catch(ValidationException exception) {
				await botUIService.SendValidationError(user, exception);
			} catch (ArgumentException exception) {
				//TODO: Log exception
				throw;
			}

			await HandleNextStage(user, nextAccountAssemblingStage);
		}

		private async Task HandleNextStage(BotUser user, AccountAssemblingStage nextAccountAssemblingStage) {
			if (nextAccountAssemblingStage == AccountAssemblingStage.Release) {
				var account = accountAssemblingService.Release(user.Id);
				if (await accountService.AddAccountAsync(user.Id, account)) {
					//TODO: use emoji by key
					await botUIService.ShowAccount(user, account,
						extraMessage: "✅ " + String.Format(Localization.GetMessage("AccountAdded", user.Lang),
							account.AccountName));
				}
				await userService.UpdateActionAsync(user.Id, UserAction.Search);
			} else {
				if (user.Action != UserAction.AssembleAccount)
					await userService.UpdateActionAsync(user.Id, UserAction.AssembleAccount);
				await SendNextStageInstruction(user, nextAccountAssemblingStage);
			}
		}

		//TODO:
		//Use emoji from new emoji system
		//and use new localization sysrtem where there are methods with parameters
		//and get buttons from UI bot service
		private async Task SendNextStageInstruction(BotUser user, AccountAssemblingStage nextAccountAssemblingStage, int? messageToEditId = null) {
			(string message, InlineKeyboardMarkup replyMarkup) = nextAccountAssemblingStage switch {
				AccountAssemblingStage.AddAccountName
					=> ("📝 " + Localization.GetMessage("AddAccount", user.Lang), null),
				AccountAssemblingStage.AddLink
					=> ((Func<(string, InlineKeyboardMarkup)>)(() => {
						string autoLink = accountAssemblingService.GetAccountName(user.Id).AutoDomain();
						return ("🔗 " + Localization.GetMessage("AddLink", user.Lang),
						new InlineKeyboardMarkup(
							new InlineKeyboardButton[][] {
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										"🔗 " + autoLink,
										AddAccountCommandCode.AutoLink.ToStringCode() + autoLink)},
								new InlineKeyboardButton[] {
									InlineKeyboardButton.WithCallbackData(
										"⏩ " + Localization.GetMessage("Skip",user.Lang),
										AddAccountCommandCode.SkipLink.ToStringCode())}})); }))(),
				AccountAssemblingStage.AddNote
					=> ("🗒 " + Localization.GetMessage("AddNote", user.Lang),
						new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(
							"⏩ " + Localization.GetMessage("Skip", user.Lang),
							AddAccountCommandCode.SkipNote.ToStringCode()))),
				AccountAssemblingStage.AddLogin
					=> ("📇 " + Localization.GetMessage("AddLogin", user.Lang), null),
				AccountAssemblingStage.AddPassword
					=> ("🔑 " + Localization.GetMessage("AddPassword", user.Lang),
							botUIService.GeneratePasswordKeyboard(user,
								GeneratePasswordCommandCode.Assembling,
								SetUpPasswordGeneratorCommandCode.ReturnAssembling)),
				AccountAssemblingStage.AddEncryptionKey
					=> ("🔐 " + Localization.GetMessage("AddEncryptionKey", user.Lang),
						new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(
							"⏩ " + Localization.GetMessage("Skip", user.Lang),
							AddAccountCommandCode.SkipEncryptionKey.ToStringCode()))),
				_ => throw new ArgumentException("Unexcpected AccountAssemblingStage")
			};

			if (messageToEditId == null) {
				await bot.Client
					.SendTextMessageAsync(user.Id, message,
						replyMarkup: replyMarkup,
						parseMode: ParseMode.Markdown);
			} else {
				await bot.Client.EditMessageTextAsync(
					user.Id, messageToEditId.Value, message,
					replyMarkup: replyMarkup,
					parseMode: ParseMode.Markdown);
			}
		}

		//Moved from password manager
		private async Task ReportExceededLength(BotUser user, int maxAccountDataLength, string accountDataType) {
			await bot.Client.SendTextMessageAsync(user.Id,
				String.Format(Localization.GetMessage("MaxLength", user.Lang),
					Localization.GetMessage(accountDataType, user.Lang),
						maxAccountDataLength));
		}

		//TODO: Refactor
		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
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
					? accountAssemblingService.SkipStage(user.Id,
						(AccountAssemblingStageSkip)assemblingStage)
					: accountAssemblingService.Assemble(user.Id,
						propertyToAssemble, assemblingStage);
				await SendNextStageInstruction(user, nextStage,
					callbackQuery.Message.MessageId);
			} catch (ValidationException exception) {
				await botUIService.SendValidationError(user, exception);
			} catch (InvalidOperationException) {
				await ReportAbsenceOfNewAccount(user, callbackQuery.Id);
			} catch (Exception exception) {
				//TODO: Log Exception
				throw;
			}

			
		}

		private async Task ReportAbsenceOfNewAccount(BotUser user, string callbackQueryId)
			=> await bot.Client.AnswerCallbackQueryAsync(callbackQueryId,
				text: Localization.GetMessage("CantWithoutNewAcc", user.Lang), showAlert: true);
		

		async Task IActionCommand.ExecuteAsync(Message message, BotUser user) {
			AccountAssemblingStage nextAssemblingStage = AccountAssemblingStage.None;
			try {
				nextAssemblingStage = accountAssemblingService.Assemble(user.Id, message.Text);
			} catch (ValidationException exception) {
				await botUIService.SendValidationError(user, exception);
			} catch (InvalidOperationException) {
				await userService.UpdateActionAsync(user.Id, UserAction.Search);
			}

			await HandleNextStage(user, nextAssemblingStage);
		}

		async Task IReplyActionCommand.ExecuteAsync(Message message, BotUser user) {
			//This command allows to bypass pressing "Accept password" button and than entering encryption key
			//by sending encryption key right in reply to suggested password message
			if (message.ReplyToMessage.ReplyMarkup != null
				&& message.ReplyToMessage.ReplyMarkup.InlineKeyboard
					.Any(x => x.Any(y => !string.IsNullOrEmpty(y.CallbackData)
						&& y.CallbackData == AddAccountCommandCode.AcceptPassword.ToStringCode()))) {
				AccountAssemblingStage nextAssemblingStage = AccountAssemblingStage.None;
				try {
					accountAssemblingService.Assemble(user.Id, message.ReplyToMessage.Text,
						AccountAssemblingStage.AddPassword);
					nextAssemblingStage = accountAssemblingService
						.Assemble(user.Id, message.Text, AccountAssemblingStage.AddEncryptionKey);
				} catch (ValidationException exception) {
					await botUIService.SendValidationError(user, exception);
				} catch (InvalidOperationException) {
					await userService.UpdateActionAsync(user.Id, UserAction.Search);
				}

				await HandleNextStage(user, nextAssemblingStage);
			} else {
				//handle reply action command as regular action command
				await (this as IActionCommand).ExecuteAsync(message, user);
			}
		}
	}
}
