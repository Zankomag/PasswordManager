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
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using AutoMapper;

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
			long? accountId = null) {

			if(nextAccountUpdatingStage == AccountUpdatingStage.Release) {
				Account updatingAccount = accountUpdatingService.GetAccount(user.Id);
				accountUpdatingService.FinishUpdatingRequest(user.Id);
				Account account = await accountService.GetFullAsync(user.Id, updatingAccount.Id);
				if((account != null) && ((accountId == null) || (accountId == account.Id))) {
					mapper.Map(updatingAccount, account);
				}
				await userService.UpdateActionAsync(user.Id, UserAction.Search);
			} else if(nextAccountUpdatingStage == AccountUpdatingStage.None){
				await userService.UpdateActionAsync(user.Id, UserAction.Search);
			} else {
			
			}
		}

		//TODO:
		//use emoji from new system
		//
		//TODO:
		//instead of sending new message with invintation bot have
		//to edit message with selection to message with invintation
		//WITH "BACK" button that returns to selection message
		//and after users sends new data - bot have to delete only one message
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
				case UpdateAccountCommandCode.AccountName:
					if (account != null) {
						string message = await botUIService.SerializeAccount(user, account, false,
						"📝 " + string.Format(Localization.GetMessage("UpdateAccData", user.Lang),
							Localization.GetMessage("NewAccountName", user.Lang)));
						await bot.Client.EditMessageTextAsync(user.Id, callbackQuery.Message.MessageId, message,
							replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(
								"⬅️ " + Localization.GetMessage("Back", user.Lang),
								UpdateAccountCommandCode.SelectUpdateType.ToStringCode(accountId))));
						accountUpdatingService.StartUpdatingRequest(user.Id, account, AccountUpdatingStage.AccountName);
						await userService.UpdateActionAsync(user.Id, UserAction.UpdateAccount);
					}
					break;
				case UpdateAccountCommandCode.Link:
					if (account != null) {
						string message = await botUIService.SerializeAccount(user, account, false,
						"🔗 " + string.Format(Localization.GetMessage("UpdateAccData", user.Lang),
							Localization.GetMessage("NewLink", user.Lang)));
						await bot.Client.EditMessageTextAsync(user.Id, callbackQuery.Message.MessageId, message,
							replyMarkup: new InlineKeyboardMarkup(
								new InlineKeyboardButton[]{
									InlineKeyboardButton.WithCallbackData(
										"⬅️ " + Localization.GetMessage("Back", user.Lang),
										UpdateAccountCommandCode.SelectUpdateType.ToStringCode(accountId)),
									InlineKeyboardButton.WithCallbackData(
										"🗑 " + Localization.GetMessage("DeleteLink", user.Lang),
										UpdateAccountCommandCode.DeleteLink.ToStringCode(accountId)) }));
						accountUpdatingService.StartUpdatingRequest(user.Id, account, AccountUpdatingStage.Link);
						await userService.UpdateActionAsync(user.Id, UserAction.UpdateAccount);
					}
					break;
				case UpdateAccountCommandCode.Note:
					if (account != null) {
						string message = await botUIService.SerializeAccount(user, account, false,
						"🗒 " + string.Format(Localization.GetMessage("UpdateAccData", user.Lang),
							Localization.GetMessage("NewNote", user.Lang)));
						await bot.Client.EditMessageTextAsync(user.Id, callbackQuery.Message.MessageId, message,
							replyMarkup: new InlineKeyboardMarkup(
								new InlineKeyboardButton[]{
									InlineKeyboardButton.WithCallbackData(
										"⬅️ " + Localization.GetMessage("Back", user.Lang),
										UpdateAccountCommandCode.SelectUpdateType.ToStringCode(accountId)),
									InlineKeyboardButton.WithCallbackData(
										"🗑 " + Localization.GetMessage("DeleteNote", user.Lang),
										UpdateAccountCommandCode.DeleteNote.ToStringCode(accountId)) }));
						accountUpdatingService.StartUpdatingRequest(user.Id, account, AccountUpdatingStage.Note);
						await userService.UpdateActionAsync(user.Id, UserAction.UpdateAccount);
					}
					break;
				case UpdateAccountCommandCode.Login:
					if (account != null) {
						string message = await botUIService.SerializeAccount(user, account, false,
						"📇 " + string.Format(Localization.GetMessage("UpdateAccData", user.Lang),
							Localization.GetMessage("NewLogin", user.Lang)));
						await bot.Client.EditMessageTextAsync(user.Id, callbackQuery.Message.MessageId, message,
							replyMarkup: new InlineKeyboardMarkup(
								new InlineKeyboardButton[]{
									InlineKeyboardButton.WithCallbackData(
										"⬅️ " + Localization.GetMessage("Back", user.Lang),
										UpdateAccountCommandCode.SelectUpdateType.ToStringCode(accountId))}));
						accountUpdatingService.StartUpdatingRequest(user.Id, account, AccountUpdatingStage.Login);
						await userService.UpdateActionAsync(user.Id, UserAction.UpdateAccount);
					}
					break;
				case UpdateAccountCommandCode.Password:
					if (account != null) {
						string message = await botUIService.SerializeAccount(user, account, false,
						"📇 " + string.Format(Localization.GetMessage("UpdateAccData", user.Lang),
							Localization.GetMessage("NewPassword", user.Lang)));
						await bot.Client.EditMessageTextAsync(user.Id, callbackQuery.Message.MessageId, message,
							replyMarkup: new InlineKeyboardMarkup(
								new InlineKeyboardButton[][] {
									new InlineKeyboardButton[]{
										InlineKeyboardButton.WithCallbackData(
											"⬅️ " + Localization.GetMessage("Back", user.Lang),
											UpdateAccountCommandCode.SelectUpdateType.ToStringCode(accountId)) },
									botUIService.GeneratePasswordKeyboard(user, GeneratePasswordCommandCode.Updating,
										SetUpPasswordGeneratorCommandCode.ReturnUpdating, accountId)}));
						accountUpdatingService.StartUpdatingRequest(user.Id, account, AccountUpdatingStage.Password);
						await userService.UpdateActionAsync(user.Id, UserAction.UpdateAccount);
					}
					break;
				case UpdateAccountCommandCode.DeleteLink:
					try {
						nextUpdatingStage = accountUpdatingService.GetNextUpdatingStage(user.Id, null,
							AccountUpdatingStage.Link, accountId);
						await HandleNextUpdatingStage(user, accountId, nextUpdatingStage, updateAccountCommandCode);
					} catch (ValidationException exception) {
						await botUIService.SendValidationError(user, exception);
					}
					break;
				case UpdateAccountCommandCode.DeleteNote:
					try {
						nextUpdatingStage = accountUpdatingService.GetNextUpdatingStage(user.Id, null,
							AccountUpdatingStage.Note, accountId);
						await HandleNextUpdatingStage(user, accountId, nextUpdatingStage, updateAccountCommandCode);
					} catch (ValidationException exception) {
						await botUIService.SendValidationError(user, exception);
					}
					break;
				case UpdateAccountCommandCode.OutdatedTime:
					if (account != null) {
						string message = await botUIService.SerializeAccount(user, account, false,
						"🕜 " + string.Format(Localization.GetMessage("UpdateAccData", user.Lang),
							Localization.GetMessage("NewOutdatedTime", user.Lang)));
						await bot.Client.EditMessageTextAsync(user.Id, callbackQuery.Message.MessageId, message,
							replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(
								"⬅️ " + Localization.GetMessage("Back", user.Lang),
								UpdateAccountCommandCode.SelectUpdateType.ToStringCode(accountId))));
						accountUpdatingService.StartUpdatingRequest(user.Id, account, AccountUpdatingStage.OutdatedTime);
						await userService.UpdateActionAsync(user.Id, UserAction.UpdateAccount);
					}
					break;
				case UpdateAccountCommandCode.UntrackOutdatedTime:
					if (account != null) {
						accountUpdatingService.StartUpdatingRequest(user.Id, account, AccountUpdatingStage.OutdatedTime);
						try {
							nextUpdatingStage = accountUpdatingService.GetNextUpdatingStage(user.Id, "0",
								AccountUpdatingStage.OutdatedTime, accountId);
							await HandleNextUpdatingStage(user, accountId, nextUpdatingStage, updateAccountCommandCode);
						} catch (ValidationException exception) {
							await botUIService.SendValidationError(user, exception);
						}
					}
					break;
				case UpdateAccountCommandCode.UseGlobalOutdatedTime:
					if (account != null) {
						accountUpdatingService.StartUpdatingRequest(user.Id, account, AccountUpdatingStage.OutdatedTime);
						try {
							nextUpdatingStage = accountUpdatingService.GetNextUpdatingStage(user.Id, null,
								AccountUpdatingStage.OutdatedTime, accountId);
							await HandleNextUpdatingStage(user, accountId, nextUpdatingStage, updateAccountCommandCode);
						} catch (ValidationException exception) {
							await botUIService.SendValidationError(user, exception);
						}
					}
					break;
				case UpdateAccountCommandCode.RemoveEncryption:
					if (account != null) {
						accountUpdatingService.StartUpdatingRequest(user.Id, account, AccountUpdatingStage.SkipPasswordEncryption);
						try {
							nextUpdatingStage = accountUpdatingService.GetNextUpdatingStage(user.Id, callbackQuery.Message.Text,
								AccountUpdatingStage.SkipPasswordEncryption, accountId);
							await HandleNextUpdatingStage(user, accountId, nextUpdatingStage, updateAccountCommandCode);
						} catch (ValidationException exception) {
							await botUIService.SendValidationError(user, exception);
						}
					}
					break;
				case UpdateAccountCommandCode.AcceptPassword:
					if (account != null) {
						try {
							nextUpdatingStage = accountUpdatingService.GetNextUpdatingStage(user.Id,
								callbackQuery.Message.Text, AccountUpdatingStage.Password, accountId);
							await HandleNextUpdatingStage(user, accountId, nextUpdatingStage, updateAccountCommandCode);
						} catch (ValidationException exception) {
							await botUIService.SendValidationError(user, exception);
						}
					}
					break;
			}
		}

		async Task IActionCommand.ExecuteAsync(Message message, BotUser user) {
			AccountUpdatingStage nextUpdatingStage = AccountUpdatingStage.None;
			try {
				nextUpdatingStage = accountUpdatingService.GetNextUpdatingStage(user.Id, message.Text);
			} catch (ValidationException exception) {
				await botUIService.SendValidationError(user, exception);
			} catch (InvalidOperationException) {
				await userService.UpdateActionAsync(user.Id, UserAction.Search);
			}

			await HandleNextUpdatingStage(user, accountId, nextUpdatingStage, nextUpdatingStage);
		}

		async Task IReplyActionCommand.ExecuteAsync(Message message, BotUser user) {
			aaa
		}

	}
}