using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot;
using MultiUserLocalization;
using PasswordManager.Bot.Enums;
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
				if (.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
					//TODO:
					//ENCRYPT PASSWORD
					account.Password = callbackQuery.Message.Text;
					.AssemblingAccounts[user.Id] = account;
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
			else if(.UpdatingAccounts.ContainsKey(callbackQuery.From.Id)){
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id);
				await .UpdateAccountDataAsync(callbackQuery.Message.Text, .UpdatingAccounts[callbackQuery.From.Id].AccountToUpdateId, callbackQuery.From.Id, user.Lang);
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
