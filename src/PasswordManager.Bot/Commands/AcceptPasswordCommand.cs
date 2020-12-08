using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Types.Enums;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Application.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class AcceptPasswordCommand : Abstractions.BotCommand, ICallbackQueryCommand {
		private readonly IAccountService accountService;

		public AcceptPasswordCommand(IBotService botService, IAccountService accountService) : base(botService) {
			this.accountService = accountService;
		}


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
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id);
				await PasswordManagerService.UpdateAccountDataAsync(callbackQuery.Message.Text, PasswordManagerService.UpdatingAccounts[callbackQuery.From.Id].AccountToUpdateId, callbackQuery.From.Id, user.Lang);
				return;
			}
			await AnswerWithWarning(callbackQuery.Id, user.Lang);
		}

		private async Task AnswerWithWarning(string callbackQueryId, string langCode) {
			await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQueryId,
						text: Localization.GetMessage("CantWithoutNewAcc", langCode), showAlert: true);
		}
	}
}
