using System.Threading.Tasks;
using Telegram.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class HelpCommand : Abstractions.BotCommand, IMessageCommand {

		public HelpCommand(IBot botService) : base(botService) { }

		async Task IMessageCommand.ExecuteAsync(Message message, BotUser user) {
			//TODO:
			//Create public static method in HelpCommand that returns help message
			await BotHandler.Bot.SendTextMessageAsync(message.From.Id, 
				string.Format(Localization.GetMessage("Help", user.Lang), 
				"/add", "/all", "/generator", "/language", "/cancel","/help"), Telegram.Bot.Types.Enums.ParseMode.Markdown);
		}
	}
}
