using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class HelpCommand : Abstractions.BotCommand, IMessageCommand {

		public HelpCommand(IBot bot) : base(bot) { }

		async Task IMessageCommand.ExecuteAsync(Message message, BotUser user)
			//TODO:
			//Create public static method in HelpCommand that returns help message
			=> await Bot.Client.SendTextMessageAsync(message.From.Id, 
				String.Format(Localization.GetMessage("Help", user.Lang), 
				"/add", "/all", "/generator", "/language", "/cancel","/help"),
				Telegram.Bot.Types.Enums.ParseMode.Markdown);
	}
}
