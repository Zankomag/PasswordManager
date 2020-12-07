using System.Threading.Tasks;
using Telegram.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class HelpCommand : IMessageCommand {
		public async Task ExecuteAsync(Message message, BotUser user) {
			await BotHandler.Bot.SendTextMessageAsync(message.From.Id, 
				string.Format(Localization.GetMessage("Help", user.Lang), 
				"/add", "/all", "/generator", "/language", "/cancel","/help"), Telegram.Bot.Types.Enums.ParseMode.Markdown);
		}
	}
}
