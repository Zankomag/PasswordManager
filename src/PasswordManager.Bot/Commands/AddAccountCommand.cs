using System;
using System.Data;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MultiUserLocalization;
using PasswordManager.Bot.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using System.ComponentModel.DataAnnotations;
using PasswordManager.Bot.Commands.Enums;
using Telegram.Bot.Types.Enums;

namespace PasswordManager.Bot.Commands {
	public class AddAccountCommand : Abstractions.BotCommand, IMessageCommand, IActionCommand, ICallbackQueryCommand {
		private readonly IAccountService accountService;
		private readonly IUserService userService;
		private readonly IAccountAssemblingService accountAssemblingService;
		private readonly IBotUIService botUIService;

		public AddAccountCommand(IBotService botService, IAccountService accountService,
			IUserService userService, IAccountAssemblingService accountAssemblingService,
			IBotUIService botUIService) : base(botService)
			=> (this.accountService, this.userService, this.accountAssemblingService, this.botUIService)
				= (	 accountService,	  userService,		accountAssemblingService,	   botUIService);

		async Task IMessageCommand.ExecuteAsync(Message message, BotUser user) {
			AccountAssemblingStage nextAccountAssemblingStage = AccountAssemblingStage.None;
			try {
				nextAccountAssemblingStage = accountAssemblingService
				.Create(user.Id, message.Text.GetCommandArgsByNewLine());
			} catch(ValidationException exception) {
				//TODO:
				//Change to good translated message
				await botService.Client.SendTextMessageAsync(user.Id, exception.Message);
			} catch (ArgumentException exception) {
				//TODO: Log exception
				throw;
			}

			if(nextAccountAssemblingStage == AccountAssemblingStage.Release) {
				var account = accountAssemblingService.Release(user.Id);
				if(await accountService.AddAccountAsync(user.Id, account)) {
					//TODO: use emoji by key
					await botUIService.ShowAccount(user, account,
						extraMessage: "✅ " + String.Format(Localization.GetMessage("AccountAdded", user.Lang),
							account.AccountName));
				}
				if (user.Action == UserAction.AssembleAccount)
					await userService.UpdateActionAsync(user.Id, UserAction.Search);
			} else {
				if (user.Action != UserAction.AssembleAccount)
					await userService.UpdateActionAsync(user.Id, UserAction.AssembleAccount);
				await SendNextStageInstruction(user, nextAccountAssemblingStage);
			}
		}

		//This method exists because in C# you cannot use raw tuple assigment
		//in the lambda right-hand-side of switch expression
		private (string, InlineKeyboardMarkup) GetMessageTuple(string message, InlineKeyboardMarkup replyMarkup)
			=> (message, replyMarkup);

		//TODO:
		//Use emoji from new emoji system
		//and use new localization sysrtem where there is methods with parameters
		//and get buttons from UI bot service
		private async Task SendNextStageInstruction(BotUser user, AccountAssemblingStage nextAccountAssemblingStage, int? messageToEditId = null) {
			(string message, InlineKeyboardMarkup replyMarkup) = nextAccountAssemblingStage switch {
				AccountAssemblingStage.AddAccountName 
					=> GetMessageTuple("📝 " + Localization.GetMessage("AddAccount", user.Lang), null),
				AccountAssemblingStage.AddLink
					=> GetMessageTuple("🔗 " + Localization.GetMessage("AddLink", user.Lang),
							new InlineKeyboardMarkup(
								new InlineKeyboardButton[][] {
									new InlineKeyboardButton[] {
										InlineKeyboardButton.WithCallbackData(
											"🔗 " + accountAssemblingService.GetAccountName(user.Id)
												.AutoLink().BuildLink(),
											AddAccountCommandCode.AutoLink.ToStringCode())},
									new InlineKeyboardButton[] {
										InlineKeyboardButton.WithCallbackData(
											"⏩ " + Localization.GetMessage("Skip",user.Lang),
											AddAccountCommandCode.SkipLink.ToStringCode())}})),
				AccountAssemblingStage.AddNote
					=> GetMessageTuple("🗒 " + Localization.GetMessage("AddNote", user.Lang),
						new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(
							"⏩ " + Localization.GetMessage("Skip", user.Lang),
							AddAccountCommandCode.SkipLink.ToStringCode()))),
				AccountAssemblingStage.AddLogin
					=> GetMessageTuple("📇 " + Localization.GetMessage("AddLogin", user.Lang), null),
				AccountAssemblingStage.AddPassword
					=> GetMessageTuple("🔐 " + String.Format(
						Localization.GetMessage("AddPassword", user.Lang), "/generator"),
						new InlineKeyboardMarkup(InlineKeyboardButton
							.WithCallbackData("🌋 " + Localization.GetMessage("Generate", user.Lang),
								CallbackQueryCommandCode.GeneratePassword.ToStringCode()))),
				AccountAssemblingStage.AddEncryptionKey
					=> GetMessageTuple("📇 " + Localization.GetMessage("AddEncryptionKey", user.Lang), null),
				_ => throw new ArgumentException("Unexcpected AccountAssemblingStage")
			};

			if (messageToEditId == null) {
				await botService.Client
					.SendTextMessageAsync(user.Id, message,
						replyMarkup: replyMarkup,
						parseMode: ParseMode.Markdown);
			} else {
				await botService.Client.EditMessageTextAsync(
					user.Id, messageToEditId.Value, message,
					replyMarkup: replyMarkup,
					parseMode: ParseMode.Markdown);
			}
		}

		//Moved from password manager
		private async Task ReportExceededLength(ChatId chatid, string langCode, MaxAccountDataLength maxAccountDataLength) {
			await botService.Client.SendTextMessageAsync(chatid,
				String.Format(Localization.GetMessage("MaxLength", langCode), Localization.GetMessage(maxAccountDataLength.ToString(), langCode), (int)maxAccountDataLength));
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

			if (.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
				switch (addAccountCommandCode) {
					case AddAccountCommandCode.SkipLink:
						try {
							var nextStage = accountAssemblingService
								.SkipStage(user.Id, AccountAssemblingStageSkip.AddLink);
							await SendNextStageInstruction(user, nextStage,
								callbackQuery.Message.MessageId);
						} catch (InvalidOperationException) { }
						break;
					case CallbackQueryCommandCode.AcceptPassword:
						//TODO:
						//ENCRYPT PASSWORD
						account.Password = callbackQuery.Message.Text;
						.AssemblingAccounts[user.Id] = account;
						await AddAccountCommand.UpdateCallBackMessageAsync(
							callbackQuery.Message.Chat.Id,
							callbackQuery.Message.MessageId,
							account,
							user);
						break;
				}
				
			} else {
				await AnswerNewAccountAbsence(callbackQuery.Id, user.Lang);
			}
		}

		private async Task AnswerNewAccountAbsence(string callbackQueryId, string langCode) {
			await botService.Client.AnswerCallbackQueryAsync(callbackQueryId,
						text: Localization.GetMessage("CantWithoutNewAcc", langCode), showAlert: true);
		}

		async Task IActionCommand.ExecuteAsync(Message message, BotUser user) => _eRROR_ throw new NotImplementedException();
	}
}
