﻿using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Types.Enums;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Models;

namespace PasswordManager.Bot.Commands {
	public class AcceptPasswordCommand : ICallbackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			if (user.Action == UserAction.AssembleAccount) {
				if (PasswordManagerService.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
					//TODO:
					//ENCRYPT PASSWORD
					account.Password = callbackQuery.Message.Text;
					PasswordManagerService.AssemblingAccounts[user.Id] = account;
					await AddAccountCommand.UpdateCallBackMessageAsync(
						callbackQuery.Message.Chat.Id,
						callbackQuery.Message.MessageId,
						account,
						user);
				} else {
					await AnswerWithWarning(callbackQuery.Id, user.Lang);
				}
				return;
			}
			else if(PasswordManagerService.UpdatingAccounts.ContainsKey(callbackQuery.From.Id)){
				await BotHandlerService.Bot.AnswerCallbackQueryAsync(callbackQuery.Id);
				await PasswordManagerService.UpdateAccountDataAsync(callbackQuery.Message.Text, PasswordManagerService.UpdatingAccounts[callbackQuery.From.Id].AccountToUpdateId, callbackQuery.From.Id, user.Lang);
				return;
			}
			await AnswerWithWarning(callbackQuery.Id, user.Lang);
		}

		private async Task AnswerWithWarning(string callbackQueryId, string langCode) {
			await BotHandlerService.Bot.AnswerCallbackQueryAsync(callbackQueryId,
						text: Localization.GetMessage("CantWithoutNewAcc", langCode), showAlert: true);
		}
	}
}
