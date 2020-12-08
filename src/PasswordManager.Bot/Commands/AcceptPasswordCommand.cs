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
	//TODO:
	//Move command logic to AddAccountCommand
	public class AcceptPasswordCommand : Abstractions.BotCommand, ICallbackQueryCommand {
		private readonly IAccountService accountService;

		public AcceptPasswordCommand(IBotService botService, IAccountService accountService) : base(botService) {
			this.accountService = accountService;
		}

		//TODO: Move logic to update account
		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			if (user.Action == UserAction.AssembleAccount) {

			}
			else if(.UpdatingAccounts.ContainsKey(callbackQuery.From.Id)){
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id);
				await .UpdateAccountDataAsync(callbackQuery.Message.Text, .UpdatingAccounts[callbackQuery.From.Id].AccountToUpdateId, callbackQuery.From.Id, user.Lang);
				return;
			}
			
		}

	
	}
}
